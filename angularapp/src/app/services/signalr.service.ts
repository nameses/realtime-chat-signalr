import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, from } from 'rxjs';
import { connect, tap } from 'rxjs/operators';
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
  readonly POST_PRIVATE_URL = 'https://localhost:7161/api/chat/send/private';

  private receivedMessageObject: ChatMessage = new ChatMessage();
  private sharedObj = new Subject<ChatMessage>();

  private connectionId: string | undefined;

  constructor(private http: HttpClient) {
    this.connection.onclose(async () => {
      await this.start();
    });
    this.connection.on('ReceiveMessage', (user: string, message: string) => {
      this.mapReceivedMessage(user, message);
    });
    this.connection.on(
      'ReceivePrivateMessage',
      (user: string, message: string, receiverConnectionId: string) => {
        this.mapReceivedMessage(user, message, true);
      }
    );
    this.start();
  }

  // Starrt the connection
  public async start() {
    try {
      await this.connection.start();
      console.log('Successfully connected.');
    } catch (err) {
      console.log(err);
      setTimeout(() => this.start(), 5000);
    }
    this.connection.invoke('GetConnectionID').then((id: any) => {
      this.connectionId = id;
      console.log('connection id = ' + this.connectionId);
    });
  }

  private mapReceivedMessage(
    user: string,
    message: string,
    ifPrivate?: boolean
  ): void {
    this.receivedMessageObject.user = user;
    this.receivedMessageObject.msgText = message;
    if (ifPrivate) this.receivedMessageObject.ifPrivate = ifPrivate;

    this.sharedObj.next(this.receivedMessageObject);
  }

  public sendMessageToUser(msgDto: ChatMessage, receiverConnectionId: string) {
    console.log('private message to user ' + receiverConnectionId);
    console.log(msgDto);
    this.http
      .post(this.POST_PRIVATE_URL, {
        user: msgDto.user,
        msgText: msgDto.msgText,
        receiverConnectionId: receiverConnectionId,
      })
      .subscribe((data) => console.log(data));
  }

  public broadcastMessage(msgDto: ChatMessage) {
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
