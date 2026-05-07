import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ChatService } from '../../../core/services/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { Conversation } from '../../../core/models/chat.model';

@Component({
  selector: 'app-new-conversation-dialog',
  templateUrl: './new-conversation-dialog.component.html',
  styleUrls: ['./new-conversation-dialog.component.scss']
})
export class NewConversationDialogComponent implements OnInit {
  @Input() mode: 'direct' | 'group' = 'direct';
  @Output() cancel = new EventEmitter<void>();
  @Output() created = new EventEmitter<Conversation>();

  users: UserListItem[] = [];
  selected: Set<string> = new Set();
  searchTerm = '';
  groupName = '';
  loading = false;
  submitting = false;

  private currentUserId: string | null;

  constructor(
    private chat: ChatService,
    private auth: AuthService,
    private users$: UserManagementService,
    private toast: ToastService
  ) {
    this.currentUserId = this.auth.currentUser?.id ?? null;
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.users$.getUsers({ pageSize: 100, isActive: true }).subscribe({
      next: result => {
        this.users = result.items.filter(u => u.id !== this.currentUserId);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toast.error('Không tải được danh sách người dùng.');
      }
    });
  }

  get filteredUsers(): UserListItem[] {
    const q = this.searchTerm.trim().toLowerCase();
    if (!q) return this.users;
    return this.users.filter(u =>
      u.fullName.toLowerCase().includes(q)
      || u.email.toLowerCase().includes(q)
    );
  }

  toggleUser(user: UserListItem): void {
    if (this.mode === 'direct') {
      this.selected = new Set([user.id]);
    } else {
      if (this.selected.has(user.id)) {
        this.selected.delete(user.id);
      } else {
        this.selected.add(user.id);
      }
      this.selected = new Set(this.selected);
    }
  }

  isSelected(user: UserListItem): boolean {
    return this.selected.has(user.id);
  }

  get canSubmit(): boolean {
    if (this.submitting) return false;
    if (this.mode === 'direct') {
      return this.selected.size === 1;
    }
    return this.selected.size >= 1 && this.groupName.trim().length > 0;
  }

  submit(): void {
    if (!this.canSubmit) return;
    this.submitting = true;

    if (this.mode === 'direct') {
      const otherUserId = Array.from(this.selected)[0];
      this.chat.createDirect({ otherUserId }).subscribe({
        next: conv => {
          this.submitting = false;
          this.created.emit(conv);
        },
        error: () => {
          this.submitting = false;
          this.toast.error('Không tạo được cuộc trò chuyện.');
        }
      });
    } else {
      this.chat.createGroup({
        name: this.groupName.trim(),
        memberUserIds: Array.from(this.selected)
      }).subscribe({
        next: conv => {
          this.submitting = false;
          this.created.emit(conv);
        },
        error: () => {
          this.submitting = false;
          this.toast.error('Không tạo được nhóm.');
        }
      });
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
