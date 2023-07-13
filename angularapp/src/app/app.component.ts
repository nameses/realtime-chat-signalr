import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ChatService } from './services/signalr.service';
import { ChatMessage } from './models/chatMessage';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'real-time-chat';
  constructor(private chatService: ChatService) {}

  ngOnInit(): void {
    this.chatService
      .retrieveMappedObject()
      .subscribe((receivedObj: ChatMessage) => {
        this.addToInbox(receivedObj);
      }); // calls the service method to get the new messages sent
  }

  msgDto: ChatMessage = new ChatMessage();
  msgInboxArray: ChatMessage[] = [];

  send(): void {
    if (this.msgDto) {
      if (this.msgDto.user.length == 0 || this.msgDto.user.length == 0) {
        window.alert('Both fields are required.');
        return;
      } else {
        this.chatService.broadcastMessage(this.msgDto); // Send the message via a service
      }
    }
  }

  addToInbox(obj: ChatMessage) {
    let newObj = new ChatMessage();
    newObj.user = obj.user;
    newObj.msgText = obj.msgText;
    this.msgInboxArray.push(newObj);
  }
}
