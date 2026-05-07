import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ChatService } from '../../../core/services/chat.service';
import { ChatRealtimeService } from '../../../core/services/chat-realtime.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  ChatMessage,
  Conversation,
  ConversationType
} from '../../../core/models/chat.model';

@Component({
  selector: 'app-chat-page',
  templateUrl: './chat-page.component.html',
  styleUrls: ['./chat-page.component.scss']
})
export class ChatPageComponent implements OnInit, OnDestroy, AfterViewChecked {
  ConversationType = ConversationType;

  conversations: Conversation[] = [];
  activeConversation: Conversation | null = null;
  messages: ChatMessage[] = [];
  messageDraft = '';

  loadingConversations = false;
  loadingMessages = false;
  sending = false;
  searchTerm = '';

  showNewDialog = false;
  newDialogMode: 'direct' | 'group' = 'direct';

  private subs = new Subscription();
  private shouldScrollDown = false;
  private currentUserId: string | null = null;

  @ViewChild('messageList') messageList?: ElementRef<HTMLDivElement>;

  constructor(
    private chat: ChatService,
    private realtime: ChatRealtimeService,
    private auth: AuthService,
    private toast: ToastService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.currentUserId = this.auth.currentUser?.id ?? null;
  }

  ngOnInit(): void {
    this.realtime.connect().catch(() => {});

    this.subs.add(this.chat.conversations$.subscribe(list => {
      this.conversations = list;
      // Nếu conversation đang active được update từ realtime, refresh meta info.
      if (this.activeConversation) {
        const fresh = list.find(c => c.id === this.activeConversation!.id);
        if (fresh) {
          this.activeConversation = fresh;
        }
      }
    }));

    this.subs.add(this.chat.incomingMessage$.subscribe(msg => {
      if (!msg) return;
      if (this.activeConversation && msg.conversationId === this.activeConversation.id) {
        // Tránh trùng nếu message vừa send từ chính user (đã append optimistic).
        if (!this.messages.some(m => m.id === msg.id)) {
          this.messages = [...this.messages, msg];
          this.shouldScrollDown = true;
          // Đang xem conversation → tự động markRead.
          this.chat.markRead(this.activeConversation.id).subscribe({ error: () => {} });
        }
      }
    }));

    this.loadingConversations = true;
    this.chat.loadConversations().subscribe({
      next: () => {
        this.loadingConversations = false;
        const requestedId = this.route.snapshot.queryParamMap.get('id');
        if (requestedId) {
          const conv = this.conversations.find(c => c.id === requestedId);
          if (conv) this.openConversation(conv);
        }
      },
      error: () => (this.loadingConversations = false)
    });

    this.chat.getUnreadCount().subscribe({ error: () => {} });
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

  get filteredConversations(): Conversation[] {
    const q = this.searchTerm.trim().toLowerCase();
    if (!q) return this.conversations;
    return this.conversations.filter(c =>
      c.displayName.toLowerCase().includes(q)
      || (c.lastMessagePreview ?? '').toLowerCase().includes(q)
    );
  }

  openConversation(conv: Conversation): void {
    this.activeConversation = conv;
    this.messages = [];
    this.loadingMessages = true;
    this.shouldScrollDown = true;

    this.router.navigate([], {
      queryParams: { id: conv.id },
      queryParamsHandling: 'merge'
    });

    this.chat.getMessages(conv.id, 1, 50).subscribe({
      next: page => {
        // BE trả desc — reverse để hiển thị từ cũ → mới.
        this.messages = [...page.items].reverse();
        this.loadingMessages = false;
        this.shouldScrollDown = true;
      },
      error: () => (this.loadingMessages = false)
    });

    this.chat.markRead(conv.id).subscribe({ error: () => {} });
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
    const name = conv.displayName || '?';
    return name.charAt(0).toUpperCase();
  }

  groupHeaderInfo(conv: Conversation): string {
    if (conv.type === ConversationType.Group) {
      return `${conv.participants.length} thành viên`;
    }
    const other = conv.participants.find(p => p.userId !== this.currentUserId);
    return other?.email ?? '';
  }

  openNewDirect(): void {
    this.newDialogMode = 'direct';
    this.showNewDialog = true;
  }

  openNewGroup(): void {
    this.newDialogMode = 'group';
    this.showNewDialog = true;
  }

  closeNewDialog(): void {
    this.showNewDialog = false;
  }

  onConversationCreated(conv: Conversation): void {
    this.showNewDialog = false;
    this.openConversation(conv);
  }

  leaveConversation(): void {
    if (!this.activeConversation) return;
    if (!confirm('Rời khỏi cuộc trò chuyện này?')) return;
    const id = this.activeConversation.id;
    this.chat.leave(id).subscribe({
      next: () => {
        this.activeConversation = null;
        this.messages = [];
        this.toast.success('Đã rời cuộc trò chuyện.');
        this.router.navigate([], { queryParams: {} });
      },
      error: () => this.toast.error('Không rời được cuộc trò chuyện.')
    });
  }
}
