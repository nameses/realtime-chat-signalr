import { Component, OnInit } from '@angular/core';
import { ChatMessage } from 'src/app/models/chatMessage';
import { MsgType } from 'src/app/models/msgtype';
import { AccountService } from 'src/app/services/account.service';
import { GlobalChatService } from 'src/app/services/global-chat.service';
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
  //msgInboxArray: ChatMessage[] = [];

  constructor(
    private chatService: ChatService,
    public accountService: AccountService,
    public globalChatService: GlobalChatService
  ) {}

  ngOnInit(): void {
    // this.chatService
    //   .retrieveMappedObject()
    //   .subscribe((receivedObj: ChatMessage) => {
    //     this.addToInbox(receivedObj);
    //   }); // calls the service method to get the new messages sent
  }

  send(): void {
    if (this.msgDto) {
      if (this.msgDto.msgText.length == 0) {
        window.alert('Text field is empty!');
        return;
      } else {
        if (this.accountService.userValue != null) {
          this.msgDto.user = this.accountService.userValue.username;

          this.chatService.broadcastMessage(this.msgDto);

          //clear input element
          this.msgDto.msgText = '';
        }
      }
    }
  }

  addToInbox(obj: ChatMessage) {
    // let newObj = new ChatMessage();

    // newObj.user = obj.user;
    // newObj.msgType = obj.msgType;

    // if (obj.msgType == MsgType.Text) {
    //   newObj.msgText = obj.msgText;
    //   if (obj.ifPrivate) newObj.ifPrivate = obj.ifPrivate;
    // }
    // if (obj.msgType == MsgType.NewUserConnected) {
    // }
    // this.msgInboxArray.push(newObj);
    this.globalChatService.addToInbox(obj);
  }
}
