import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, from } from 'rxjs';
import { connect, tap } from 'rxjs/operators';
import { ChatMessage } from '../models/chatMessage';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { AccountService } from './account.service';
import { MsgType } from '../models/msgtype';
import { UserConnection } from '../models/userConnection';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private connection: any;
  readonly POST_URL = environment.apiUrl + '/chat/send';
  readonly POST_PRIVATE_URL = environment.apiUrl + '/chat/send/private';

  private receivedMessageObject: ChatMessage = new ChatMessage();
  private sharedObj = new Subject<ChatMessage>();

  private connectionId: string | undefined;

  private getJwtToken(): string {
    console.log(this.accountService.userValue?.token);
    if (this.accountService.userValue?.token)
      return this.accountService.userValue.token;
    throw Error();
  }

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) {
    // this.start();
  }

  public init() {
    const connectionUrl = environment.url + '/chatsocket';

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${connectionUrl}?access_token=${this.getJwtToken()}`)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onclose(async () => {
      await this.start();
    });
    this.connection.on('ReceiveMessage', (user: string, message: string) => {
      this.mapReceivedMessage(user, MsgType.Text, message, false);
    });
    this.connection.on(
      'ReceivePrivateMessage',
      (user: string, message: string, receiverUsername: string) => {
        this.mapReceivedMessage(
          user,
          MsgType.Text,
          message,
          true,
          receiverUsername
        );
      }
    );
    this.connection.on('NewUserConnected', (user: string) => {
      this.mapReceivedMessage(user, MsgType.NewUserConnected);
    });

    this.start();
  }

  // Start the connection
  public async start() {
    try {
      await this.connection
        .start()
        .then(() => {
          // Once connected, you can send additional data to the OnConnectedAsync method
          console.log(this.connection);
          this.connection.invoke(
            'OnConnectedWithUsername',
            this.accountService.userValue?.username
          );
        })
        .catch((err: any) => console.error(err));
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
    msgType: MsgType,
    message?: string,
    ifPrivate?: boolean,
    receiverUsername?: string
  ): void {
    this.receivedMessageObject.user = user;
    this.receivedMessageObject.msgType = msgType;
    if (message) this.receivedMessageObject.msgText = message;
    if (ifPrivate) {
      this.receivedMessageObject.ifPrivate = ifPrivate;
      this.receivedMessageObject.userReceiver = receiverUsername;
    }

    this.sharedObj.next(this.receivedMessageObject);
  }

  public sendMessageToUser(msgDto: ChatMessage, user: UserConnection) {
    console.log('private message to user ' + user.connectionId);
    console.log(msgDto);
    this.http
      .post(this.POST_PRIVATE_URL, {
        user: msgDto.user,
        msgText: msgDto.msgText,
        receiverConnectionId: user.connectionId,
        receiverUsername: user.username,
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
