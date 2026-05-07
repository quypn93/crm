import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import {
  AddParticipantsDto,
  ChatMessage,
  ChatMessagePage,
  Conversation,
  CreateDirectConversationDto,
  CreateGroupConversationDto,
  RenameGroupDto,
  SendMessageDto
} from '../models/chat.model';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private conversationsSubject = new BehaviorSubject<Conversation[]>([]);
  /** Cache list cho ChatLayout — đồng bộ real-time. */
  conversations$ = this.conversationsSubject.asObservable();

  private totalUnreadSubject = new BehaviorSubject<number>(0);
  totalUnread$ = this.totalUnreadSubject.asObservable();

  /** Stream message mới đến (đẩy từ realtime) — ChatRoom subscribe để append. */
  private incomingMessageSubject = new BehaviorSubject<ChatMessage | null>(null);
  incomingMessage$ = this.incomingMessageSubject.asObservable();

  constructor(private api: ApiService) {}

  loadConversations(): Observable<Conversation[]> {
    return this.api.get<Conversation[]>('chat/conversations').pipe(
      tap(items => this.conversationsSubject.next(items))
    );
  }

  getConversation(id: string): Observable<Conversation> {
    return this.api.get<Conversation>(`chat/conversations/${id}`);
  }

  createDirect(dto: CreateDirectConversationDto): Observable<Conversation> {
    return this.api.post<Conversation>('chat/conversations/direct', dto).pipe(
      tap(conv => this.upsertConversation(conv))
    );
  }

  createGroup(dto: CreateGroupConversationDto): Observable<Conversation> {
    return this.api.post<Conversation>('chat/conversations/group', dto).pipe(
      tap(conv => this.upsertConversation(conv))
    );
  }

  renameGroup(id: string, dto: RenameGroupDto): Observable<Conversation> {
    return this.api.put<Conversation>(`chat/conversations/${id}/name`, dto).pipe(
      tap(conv => this.upsertConversation(conv))
    );
  }

  addParticipants(id: string, dto: AddParticipantsDto): Observable<Conversation> {
    return this.api.post<Conversation>(`chat/conversations/${id}/participants`, dto).pipe(
      tap(conv => this.upsertConversation(conv))
    );
  }

  leave(id: string): Observable<void> {
    return this.api.post<void>(`chat/conversations/${id}/leave`, {}).pipe(
      tap(() => {
        const list = this.conversationsSubject.value.filter(c => c.id !== id);
        this.conversationsSubject.next(list);
      })
    );
  }

  getMessages(conversationId: string, page = 1, pageSize = 30): Observable<ChatMessagePage> {
    return this.api.get<ChatMessagePage>(
      `chat/conversations/${conversationId}/messages`,
      this.api.buildParams({ page, pageSize })
    );
  }

  sendMessage(conversationId: string, dto: SendMessageDto): Observable<ChatMessage> {
    return this.api.post<ChatMessage>(`chat/conversations/${conversationId}/messages`, dto);
  }

  markRead(conversationId: string): Observable<number> {
    return this.api.post<number>(`chat/conversations/${conversationId}/read`, {}).pipe(
      tap(total => {
        this.totalUnreadSubject.next(Math.max(0, total));
        const list = this.conversationsSubject.value.map(c =>
          c.id === conversationId ? { ...c, unreadCount: 0 } : c
        );
        this.conversationsSubject.next(list);
      })
    );
  }

  getUnreadCount(): Observable<number> {
    return this.api.get<number>('chat/unread-count').pipe(
      tap(count => this.totalUnreadSubject.next(Math.max(0, count)))
    );
  }

  // Realtime hooks (gọi từ ChatRealtimeService) ----------------------------

  pushIncomingMessage(message: ChatMessage): void {
    this.incomingMessageSubject.next(message);
  }

  upsertConversation(conv: Conversation): void {
    const list = this.conversationsSubject.value;
    const idx = list.findIndex(c => c.id === conv.id);
    let next: Conversation[];
    if (idx >= 0) {
      next = [...list];
      next[idx] = conv;
    } else {
      next = [conv, ...list];
    }
    // Re-sort theo lastMessageAt desc; conversation mới chưa có lastMessage → dùng createdAt.
    next.sort((a, b) => {
      const ta = new Date(a.lastMessageAt || a.createdAt).getTime();
      const tb = new Date(b.lastMessageAt || b.createdAt).getTime();
      return tb - ta;
    });
    this.conversationsSubject.next(next);
  }

  setTotalUnread(count: number): void {
    this.totalUnreadSubject.next(Math.max(0, count));
  }

  reset(): void {
    this.conversationsSubject.next([]);
    this.totalUnreadSubject.next(0);
    this.incomingMessageSubject.next(null);
  }
}
