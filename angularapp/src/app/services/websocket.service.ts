import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private socket!: WebSocket;
  //readonly SERVER_URL: string = ;

  constructor(
    private accountService: AccountService
  ) {}

  public connect(username: string): Observable<MessageEvent> {
    this.socket = new WebSocket('wss://localhost:5000/ws?username=' + username + '&access_token=' + this.getJwtToken());

    return new Observable(observer => {
      this.socket.addEventListener('open', (event) => {
        console.log('WebSocket connection opened:', event);
      });

      this.socket.addEventListener('message', (event) => {
        observer.next(event);
      });

      this.socket.addEventListener('close', (event) => {
        console.log('WebSocket connection closed:', event);
      });

      //return () => {
      //  this.socket.close();
      //};
    });
  }

  private getJwtToken(): string {
    console.log(this.accountService.userValue?.token);
    if (this.accountService.userValue?.token)
      return this.accountService.userValue.token;
    throw Error();
  }

  public sendMessage(message: string): void {
    console.log(message);
    this.socket.send(message);
  }

  public sendImage(image: File): void {
    console.log('image');
    const reader = new FileReader();
    reader.onload = (event: any) => {
      const binaryData = event.target.result;
      this.socket.send(binaryData);
    };
    console.log(image instanceof Blob);
    reader.readAsArrayBuffer(image);
  }
}
