import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ChatMessage } from 'src/app/models/chatMessage';
import { MsgType } from 'src/app/models/msgtype';
import { UserConnection } from 'src/app/models/userConnection';
import { AccountService } from 'src/app/services/account.service';
import { ChatService } from 'src/app/services/signalr.service';

@Component({
  selector: 'app-private-chat',
  templateUrl: './private-chat.component.html',
  styleUrls: ['./private-chat.component.css'],
})
export class PrivateChatComponent implements OnInit {
  public get msgType(): typeof MsgType {
    return MsgType;
  }

  title: string = 'Private chat';
  user?: UserConnection;
  msgDto: ChatMessage = new ChatMessage();
  msgInboxArray: ChatMessage[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private chatService: ChatService,
    public accountService: AccountService
  ) {}

  ngOnInit(): void {
    // Retrieve query parameters
    const username = this.route.snapshot.queryParamMap.get('username');
    const connectionId = this.route.snapshot.queryParamMap.get('connectionId');

    console.log(username, connectionId);

    if (username && connectionId)
      this.user = new UserConnection(username, connectionId);

    if (this.user?.username == this.accountService.userValue?.username)
      this.router.navigateByUrl('/users-list');
    console.log('Access to private chat with user:', username);

    this.chatService
      .retrieveMappedObject()
      .subscribe((receivedObj: ChatMessage) => {
        if (
          receivedObj.ifPrivate == true &&
          receivedObj.userReceiver == this.accountService.userValue?.username
        )
          this.addToInbox(receivedObj);
      });
  }

  send(): void {
    if (this.msgDto) {
      if (this.msgDto.msgText.length == 0) {
        window.alert('Text field is empty!');
        return;
      } else {
        if (this.accountService.userValue != null) {
          this.msgDto.user = this.accountService.userValue.username;

          this.chatService.sendMessageToUser(this.msgDto, this.user!);

          this.addToInbox({
            user: this.msgDto.user,
            msgText: this.msgDto.msgText,
            ifPrivate: true,
            msgType: MsgType.Text,
            userReceiver: this.user?.username,
          });
          //clear input element
          this.msgDto.msgText = '';
        }
      }
    }
  }

  addToInbox(obj: ChatMessage) {
    let newObj = new ChatMessage();

    // if (
    //   obj.user != this.user?.username ||
    //   obj.user != this.accountService.userValue?.username
    // )
    //   return;
    newObj.user = obj.user;
    newObj.msgType = obj.msgType;

    if (obj.msgType == MsgType.Text && obj.ifPrivate == true) {
      newObj.msgText = obj.msgText;
      newObj.ifPrivate = obj.ifPrivate;
    }
    if (obj.msgType == MsgType.NewUserConnected) {
    }
    console.log('private message: ', newObj);
    this.msgInboxArray.push(newObj);
  }
}
