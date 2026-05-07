import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatRoutingModule } from './chat-routing.module';
import { ChatPageComponent } from './chat-page/chat-page.component';
import { NewConversationDialogComponent } from './new-conversation-dialog/new-conversation-dialog.component';

@NgModule({
  declarations: [
    ChatPageComponent,
    NewConversationDialogComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ChatRoutingModule
  ]
})
export class ChatModule { }
