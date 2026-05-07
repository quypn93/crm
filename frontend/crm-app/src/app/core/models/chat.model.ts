export enum ConversationType {
  Direct = 1,
  Group = 2
}

export interface ConversationParticipant {
  userId: string;
  fullName: string;
  avatarUrl?: string;
  email?: string;
  isAdmin: boolean;
  lastReadAt?: string;
}

export interface Conversation {
  id: string;
  type: ConversationType;
  name?: string;
  displayName: string;
  createdByUserId: string;
  createdAt: string;
  lastMessageAt?: string;
  lastMessagePreview?: string;
  lastMessageSenderId?: string;
  unreadCount: number;
  participants: ConversationParticipant[];
}

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderUserId: string;
  senderName: string;
  senderAvatarUrl?: string;
  content: string;
  isDeleted: boolean;
  createdAt: string;
}

export interface ChatMessagePage {
  items: ChatMessage[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateDirectConversationDto {
  otherUserId: string;
}

export interface CreateGroupConversationDto {
  name: string;
  memberUserIds: string[];
}

export interface SendMessageDto {
  content: string;
}

export interface AddParticipantsDto {
  userIds: string[];
}

export interface RenameGroupDto {
  name: string;
}
