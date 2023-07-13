import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, from } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ChatMessage } from '../models/chatMessage';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private connection: any = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7161/chatsocket') // mapping to the chathub as in startup.cs
    .configureLogging(signalR.LogLevel.Information)
    .build();
  readonly POST_URL = 'https://localhost:7161/api/chat/send';

  private receivedMessageObject: ChatMessage = new ChatMessage();
  private sharedObj = new Subject<ChatMessage>();

  constructor(private http: HttpClient) {
    this.connection.onclose(async () => {
      await this.start();
    });
    this.connection.on('ReceiveOne', (user: string, message: string) => {
      this.mapReceivedMessage(user, message);
    });
    this.start();
  }

  // Starrt the connection
  public async start() {
    try {
      await this.connection.start();
      console.log('connected');
    } catch (err) {
      console.log(err);
      setTimeout(() => this.start(), 5000);
    }
  }

  private mapReceivedMessage(user: string, message: string): void {
    this.receivedMessageObject.user = user;
    this.receivedMessageObject.msgText = message;
    this.sharedObj.next(this.receivedMessageObject);
  }

  /* ****************************** Public Mehods **************************************** */

  // Calls the controller method
  public broadcastMessage(msgDto: any) {
    console.log(msgDto);
    this.http
      .post(this.POST_URL, msgDto)
      .subscribe((data) => console.log(data));
    // this.connection.invoke("SendMessage1", msgDto.user, msgDto.msgText).catch(err => console.error(err));
  }

  public retrieveMappedObject(): Observable<ChatMessage> {
    return this.sharedObj.asObservable();
  }
}
