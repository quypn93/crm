import {
  AfterViewChecked,
  ChangeDetectorRef,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild
} from '@angular/core';
import { Subscription } from 'rxjs';
import { ChatService } from '../../core/services/chat.service';
import { ChatRealtimeService } from '../../core/services/chat-realtime.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import {
  ChatMessage,
  ChatUser,
  Conversation,
  ConversationType
} from '../../core/models/chat.model';

type WidgetView = 'list' | 'thread' | 'new-group' | 'members';
type ListTab = 'conversations' | 'people';

@Component({
  selector: 'app-chat-widget',
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.scss']
})
export class ChatWidgetComponent implements OnInit, OnDestroy, AfterViewChecked {
  ConversationType = ConversationType;

  isOpen = false;
  isMinimized = false;
  view: WidgetView = 'list';
  tab: ListTab = 'conversations';

  conversations: Conversation[] = [];
  chatUsers: ChatUser[] = [];
  onlineUsers = new Set<string>();
  totalUnread = 0;

  activeConversation: Conversation | null = null;
  messages: ChatMessage[] = [];
  messageDraft = '';

  loadingList = false;
  loadingUsers = false;
  loadingMessages = false;
  sending = false;
  searchTerm = '';

  // State cho view 'new-group'
  groupName = '';
  groupSelected = new Set<string>();
  groupSearch = '';
  creatingGroup = false;

  private subs = new Subscription();
  private currentUserId: string | null = null;
  private shouldScrollDown = false;
  private hasLoadedOnce = false;

  @ViewChild('messageList') messageList?: ElementRef<HTMLDivElement>;

  constructor(
    private chat: ChatService,
    private realtime: ChatRealtimeService,
    private auth: AuthService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {
    this.currentUserId = this.auth.currentUser?.id ?? null;
  }

  ngOnInit(): void {
    // Connect realtime ngay từ khi user đăng nhập (widget được mount trong main layout).
    this.realtime.connect().catch(() => {});

    this.subs.add(this.chat.conversations$.subscribe(list => {
      this.conversations = list;
      // Sync active conversation nếu nó được update từ realtime.
      if (this.activeConversation) {
        const fresh = list.find(c => c.id === this.activeConversation!.id);
        if (fresh) this.activeConversation = fresh;
      }
    }));

    this.subs.add(this.chat.totalUnread$.subscribe(count => (this.totalUnread = count)));

    this.subs.add(this.chat.chatUsers$.subscribe(users => (this.chatUsers = users)));

    this.subs.add(this.chat.onlineUsers$.subscribe(set => (this.onlineUsers = set)));

    this.subs.add(this.chat.incomingMessage$.subscribe(msg => {
      if (!msg) return;
      if (this.activeConversation && msg.conversationId === this.activeConversation.id) {
        if (!this.messages.some(m => m.id === msg.id)) {
          this.messages = [...this.messages, msg];
          this.shouldScrollDown = true;
          this.chat.markRead(this.activeConversation.id).subscribe({ error: () => {} });
        }
      }
    }));

    // Pre-fetch unread count để badge hiện ngay khi load app.
    this.chat.getUnreadCount().subscribe({ error: () => {} });

    // Pre-fetch user list & conversations ngay từ đầu — khi user mở widget, data đã sẵn sàng.
    // Cũng đảm bảo presence dot hiện đúng cho conversation list ở lần mở đầu tiên.
    this.refreshUsers();
    this.chat.loadConversations().subscribe({ error: () => {} });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollDown && this.messageList) {
      const el = this.messageList.nativeElement;
      el.scrollTop = el.scrollHeight;
      this.shouldScrollDown = false;
    }
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  toggleOpen(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.isMinimized = false;
      this.ensureDataLoaded();
    }
  }

  minimize(): void {
    this.isMinimized = true;
  }

  closeWidget(): void {
    this.isOpen = false;
    this.isMinimized = false;
  }

  setTab(tab: ListTab): void {
    this.tab = tab;
    // Tab khác nhau → search semantics khác nhau, clear để tránh user bối rối
    // (vd gõ "content" tìm conversation rồi switch sang people thì filter rỗng).
    this.searchTerm = '';
    if (tab === 'people') {
      this.refreshUsers();
    }
  }

  private ensureDataLoaded(): void {
    // Conversations: load 1 lần rồi refresh khi mở lại.
    if (!this.hasLoadedOnce) {
      this.hasLoadedOnce = true;
      this.loadingList = true;
      this.chat.loadConversations().subscribe({
        next: () => (this.loadingList = false),
        error: () => (this.loadingList = false)
      });
    } else {
      this.chat.loadConversations().subscribe({ error: () => {} });
    }
    // Users: luôn refresh khi mở widget để presence đúng (signalR có thể bỏ lỡ event nếu user đóng widget lâu).
    this.refreshUsers();
  }

  usersError: string | null = null;

  refreshUsersClick(): void {
    this.refreshUsers();
  }

  private refreshUsers(): void {
    // Chỉ show spinner lần đầu — refresh nền không nên flash "đang tải".
    if (this.chatUsers.length === 0) {
      this.loadingUsers = true;
    }
    this.usersError = null;
    this.chat.loadChatUsers().subscribe({
      next: () => (this.loadingUsers = false),
      error: (err) => {
        this.loadingUsers = false;
        // Endpoint 404 thường do BE chưa restart sau khi thêm /api/chat/users.
        const status = err?.status;
        if (status === 404) {
          this.usersError = 'API /chat/users chưa có. Hãy restart backend.';
        } else if (status === 401) {
          this.usersError = 'Phiên đăng nhập đã hết hạn.';
        } else {
          this.usersError = 'Không tải được danh sách thành viên.';
        }
        console.error('[ChatWidget] loadChatUsers failed', err);
      }
    });
  }

  get filteredConversations(): Conversation[] {
    const q = this.searchTerm.trim().toLowerCase();
    if (!q) return this.conversations;
    return this.conversations.filter(c =>
      c.displayName.toLowerCase().includes(q)
      || (c.lastMessagePreview ?? '').toLowerCase().includes(q)
    );
  }

  get sortedUsers(): ChatUser[] {
    // Online trước, rồi alphabet — đọc lại từ onlineUsers set để presence change re-sort live.
    return [...this.chatUsers].sort((a, b) => {
      const aOn = this.onlineUsers.has(a.id) ? 1 : 0;
      const bOn = this.onlineUsers.has(b.id) ? 1 : 0;
      if (aOn !== bOn) return bOn - aOn;
      return a.fullName.localeCompare(b.fullName, 'vi');
    });
  }

  get filteredUsers(): ChatUser[] {
    const q = this.searchTerm.trim().toLowerCase();
    const list = this.sortedUsers;
    if (!q) return list;
    return list.filter(u =>
      u.fullName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
    );
  }

  openConversation(conv: Conversation): void {
    this.activeConversation = conv;
    this.messages = [];
    this.loadingMessages = true;
    this.shouldScrollDown = true;
    this.view = 'thread';

    this.chat.getMessages(conv.id, 1, 50).subscribe({
      next: page => {
        this.messages = [...page.items].reverse();
        this.loadingMessages = false;
        this.shouldScrollDown = true;
      },
      error: () => (this.loadingMessages = false)
    });

    this.chat.markRead(conv.id).subscribe({ error: () => {} });
  }

  /** Khởi/tái-tạo direct conversation và mở thread. */
  startDirect(user: ChatUser): void {
    this.loadingMessages = true;
    this.view = 'thread';
    this.chat.createDirect({ otherUserId: user.id }).subscribe({
      next: conv => {
        this.openConversation(conv);
      },
      error: () => {
        this.loadingMessages = false;
        this.view = 'list';
        this.toast.error('Không bắt đầu được cuộc trò chuyện.');
      }
    });
  }

  backToList(): void {
    this.view = 'list';
    this.activeConversation = null;
    this.messages = [];
    this.messageDraft = '';
  }

  // Group creation -------------------------------------------------------

  openNewGroup(): void {
    this.view = 'new-group';
    this.groupName = '';
    this.groupSelected = new Set();
    this.groupSearch = '';
    // Đảm bảo có user list để chọn.
    if (this.chatUsers.length === 0) {
      this.refreshUsers();
    }
  }

  toggleGroupMember(userId: string): void {
    // Tạo Set mới mỗi lần để Angular change detection nhận diện.
    const next = new Set(this.groupSelected);
    if (next.has(userId)) {
      next.delete(userId);
    } else {
      next.add(userId);
    }
    this.groupSelected = next;
  }

  isGroupSelected(userId: string): boolean {
    return this.groupSelected.has(userId);
  }

  get filteredGroupUsers(): ChatUser[] {
    const q = this.groupSearch.trim().toLowerCase();
    const list = this.sortedUsers;
    if (!q) return list;
    return list.filter(u =>
      u.fullName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
    );
  }

  get canCreateGroup(): boolean {
    return !this.creatingGroup
      && this.groupName.trim().length > 0
      && this.groupSelected.size >= 1;
  }

  createGroup(): void {
    if (!this.canCreateGroup) return;
    this.creatingGroup = true;
    this.chat.createGroup({
      name: this.groupName.trim(),
      memberUserIds: Array.from(this.groupSelected)
    }).subscribe({
      next: conv => {
        this.creatingGroup = false;
        this.toast.success('Đã tạo nhóm.');
        this.openConversation(conv);
      },
      error: () => {
        this.creatingGroup = false;
        this.toast.error('Không tạo được nhóm.');
      }
    });
  }

  // Members panel --------------------------------------------------------

  openMembers(): void {
    if (!this.activeConversation || this.activeConversation.type !== ConversationType.Group) return;
    this.view = 'members';
  }

  backToThread(): void {
    if (this.activeConversation) {
      this.view = 'thread';
      this.shouldScrollDown = true;
    } else {
      this.view = 'list';
    }
  }

  isParticipantOnline(userId: string): boolean {
    return this.onlineUsers.has(userId);
  }

  isCurrentUserParticipant(userId: string): boolean {
    return userId === this.currentUserId;
  }

  leaveGroup(): void {
    if (!this.activeConversation) return;
    if (!confirm(`Rời nhóm "${this.activeConversation.displayName}"?`)) return;
    const id = this.activeConversation.id;
    this.chat.leave(id).subscribe({
      next: () => {
        this.toast.success('Đã rời nhóm.');
        this.activeConversation = null;
        this.messages = [];
        this.view = 'list';
        this.tab = 'conversations';
      },
      error: () => this.toast.error('Không rời được nhóm.')
    });
  }

  sendMessage(): void {
    const content = this.messageDraft.trim();
    if (!content || !this.activeConversation || this.sending) return;

    this.sending = true;
    const conversationId = this.activeConversation.id;
    this.chat.sendMessage(conversationId, { content }).subscribe({
      next: msg => {
        if (this.activeConversation?.id === conversationId
            && !this.messages.some(m => m.id === msg.id)) {
          this.messages = [...this.messages, msg];
          this.shouldScrollDown = true;
        }
        this.messageDraft = '';
        this.sending = false;
      },
      error: () => {
        this.toast.error('Không gửi được tin nhắn.');
        this.sending = false;
      }
    });
  }

  onMessageKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  isMyMessage(msg: ChatMessage): boolean {
    return msg.senderUserId === this.currentUserId;
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  }

  formatListTime(iso?: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    const today = new Date();
    if (d.toDateString() === today.toDateString()) {
      return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }
    return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
  }

  conversationAvatarText(conv: Conversation): string {
    return (conv.displayName || '?').charAt(0).toUpperCase();
  }

  /** Direct chat → kiểm tra peer online. Group → có thành viên nào online thì coi là active. */
  isConversationOnline(conv: Conversation): boolean {
    if (conv.type === ConversationType.Direct) {
      const peer = conv.participants.find(p => p.userId !== this.currentUserId);
      return peer ? this.onlineUsers.has(peer.userId) : false;
    }
    return conv.participants.some(p =>
      p.userId !== this.currentUserId && this.onlineUsers.has(p.userId)
    );
  }

  threadHeaderSub(conv: Conversation): string {
    if (conv.type === ConversationType.Group) {
      return `${conv.participants.length} thành viên`;
    }
    return this.isConversationOnline(conv) ? 'Đang hoạt động' : 'Ngoại tuyến';
  }

  get onlineCount(): number {
    return this.chatUsers.filter(u => this.onlineUsers.has(u.id)).length;
  }
}
