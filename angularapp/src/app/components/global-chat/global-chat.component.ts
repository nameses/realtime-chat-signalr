import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ChatMessage } from 'src/app/models/chatMessage';
import { MsgType } from 'src/app/models/msgtype';
import { AccountService } from 'src/app/services/account.service';
import { ChatService } from 'src/app/services/signalr.service';

@Component({
  selector: 'app-global-chat',
  templateUrl: './global-chat.component.html',
  styleUrls: ['./global-chat.component.css'],
})
export class GlobalChatComponent implements OnInit {
  public get msgType(): typeof MsgType {
    return MsgType;
  }

  title = 'global chat';
  msgDto: ChatMessage = new ChatMessage();
  msgInboxArray: ChatMessage[] = [];
  receiverConnectionId: string | undefined;

  constructor(
    private chatService: ChatService,
    private dialog: MatDialog,
    public accountService: AccountService
  ) {}

  ngOnInit(): void {
    this.chatService
      .retrieveMappedObject()
      .subscribe((receivedObj: ChatMessage) => {
        this.addToInbox(receivedObj);
      }); // calls the service method to get the new messages sent
  }

  send(): void {
    if (this.msgDto) {
      if (this.msgDto.msgText.length == 0) {
        window.alert('Text field is empty!');
        return;
      } else {
        if (this.accountService.userValue != null) {
          this.msgDto.user = this.accountService.userValue.username;

          if (
            this.receiverConnectionId &&
            this.receiverConnectionId.trim() !== ''
          ) {
            this.chatService.sendMessageToUser(
              this.msgDto,
              this.receiverConnectionId
            );
          } else {
            this.chatService.broadcastMessage(this.msgDto);
          }

          //clear input element
          this.msgDto.msgText = '';
        }
      }
    }
  }

  addToInbox(obj: ChatMessage) {
    let newObj = new ChatMessage();

    if (obj.msgType == MsgType.Text) {
      newObj.user = obj.user;
      newObj.msgText = obj.msgText;
      if (obj.ifPrivate) newObj.ifPrivate = obj.ifPrivate;
    } else if (obj.msgType == MsgType.NewUserConnected) {
      newObj.user = obj.user;
    }
    this.msgInboxArray.push(newObj);
  }
}
