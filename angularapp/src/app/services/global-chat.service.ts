import { Injectable, OnInit } from '@angular/core';
import { ChatMessage } from '../models/chatMessage';
import { ChatService } from './signalr.service';
import { MsgType } from '../models/msgtype';

@Injectable({
  providedIn: 'root',
})
export class GlobalChatService {
  msgInboxArray: ChatMessage[] = [];
  constructor(private chatService: ChatService) {
    this.chatService
      .retrieveMappedObject()
      .subscribe((receivedObj: ChatMessage) => {
        this.addToInbox(receivedObj);
      });
  }

  addToInbox(obj: ChatMessage) {
    let newObj = new ChatMessage();

    newObj.user = obj.user;
    newObj.msgType = obj.msgType;

    if (obj.msgType == MsgType.Text) {
      newObj.msgText = obj.msgText;
      if (obj.ifPrivate) newObj.ifPrivate = obj.ifPrivate;
    }
    if (obj.msgType == MsgType.NewUserConnected) {
    }
    this.msgInboxArray.push(newObj);
  }
}
