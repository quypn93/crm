import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { StorageService } from './storage.service';
import { ChatService } from './chat.service';
import { ChatMessage, Conversation } from '../models/chat.model';

@Injectable({ providedIn: 'root' })
export class ChatRealtimeService {
  private hub: HubConnection | null = null;

  constructor(
    private storage: StorageService,
    private chat: ChatService
  ) {}

  async connect(): Promise<void> {
    if (this.hub && this.hub.state !== HubConnectionState.Disconnected) {
      return;
    }

    const token = this.storage.getToken();
    if (!token) return;

    const apiBase = environment.apiUrl.replace(/\/api\/?$/, '');
    const hubUrl = `${apiBase}/hubs/chat`;

    this.hub = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.storage.getToken() ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.hub.on('chatMessage', (msg: ChatMessage) => this.chat.pushIncomingMessage(msg));
    this.hub.on('chatConversation', (conv: Conversation) => this.chat.upsertConversation(conv));
    this.hub.on('chatUnreadCount', (count: number) => this.chat.setTotalUnread(count));

    try {
      await this.hub.start();
    } catch (err) {
      console.warn('Chat SignalR connect thất bại:', err);
    }
  }

  async disconnect(): Promise<void> {
    if (!this.hub) return;
    try {
      await this.hub.stop();
    } catch {
      // ignore
    }
    this.hub = null;
  }
}
