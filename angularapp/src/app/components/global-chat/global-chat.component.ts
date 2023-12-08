import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { ChatMessage } from 'src/app/models/chatMessage';
import { MsgType } from 'src/app/models/msgtype';
import { AccountService } from 'src/app/services/account.service';
import { GlobalChatService } from 'src/app/services/global-chat.service';
import { WebsocketService } from '../../services/websocket.service';

@Component({
  selector: 'app-global-chat',
  templateUrl: './global-chat.component.html',
  styleUrls: ['./global-chat.component.css'],
})
export class GlobalChatComponent implements OnInit {
  private socketSubscription!: Subscription;
  messages: (TextMsg | ImgMsg)[] = [];
  newMessage: string = '';
  //selectedImage!: File;
  title = 'global chat';
  //imageForm!: FormGroup;
  //selectedImageControl!: FormControl;
  file: File|null = null;

  constructor(
    public accountService: AccountService,
    public globalChatService: GlobalChatService,
    private websocketService: WebsocketService,
    private sanitizer: DomSanitizer,
    private fb: FormBuilder
  ) { }

  _arrayBufferToBase64(buffer:ArrayBuffer):string {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

  ngOnInit(): void {
    //this.imageForm = this.fb.group({
    //  selectedImage: [null],
    //});
    //this.selectedImageControl = this.imageForm.get('selectedImage') as FormControl;

    this.socketSubscription = this.websocketService.connect(this.accountService.userValue?.username!).subscribe(
      (message: MessageEvent) => {
        try {
          let parsedObject = JSON.parse(message.data);
          console.log(parsedObject);
          // Check the 'type' property to determine the message type
          if (parsedObject.msgType == 0) {
            const messageText: TextMsg = parsedObject;
            this.messages.push(messageText);
          } else if (parsedObject.msgType == 1) {
            const messageImage: ImgReqMsg = parsedObject;
            //const imageUrl = this.createImageBlobUrl(messageImage.data!);
            const imageUrl = messageImage.data!;
            const imgMsg = new ImgMsg(this.sanitizer.bypassSecurityTrustUrl('data:image/jpg;base64,'+imageUrl), messageImage.username, MsgType.Image);
            //const imgMsg = new ImgMsg(messageImage.data, messageImage.username, MsgType.Image);
            this.messages.push(imgMsg);
          }
          parsedObject=null
          return;
        } catch (error) {
          return null;
        }
      },
      (error) => {
        console.error('WebSocket error:', error);
      }
    );
  }

  ngOnDestroy(): void {
    this.socketSubscription.unsubscribe();
  }

  sendMessage(): void {
    if (this.newMessage) {
      console.log(this.newMessage)
      this.websocketService.sendMessage(this.newMessage);
      this.newMessage = '';
    }
  }

  handleFileInput(event: any) {
    this.file = event.target.files[0];
  }

  sendImage(): void {
    const selectedImage = this.file!;
    if (selectedImage) {
      this.websocketService.sendImage(selectedImage);
      //this.selectedImage = null;
    }
  }

  private createImageBlobUrl(data: ArrayBuffer): string {
    const blob = new Blob([data], { type: 'data:image/jpeg;base64,' });
    return URL.createObjectURL(blob);
  }

  isTextMessage(message: ImgMsg | TextMsg): boolean {
    return message instanceof TextMsg;
  }
}

class ImgReqMsg {
  //public data?: ArrayBuffer;
  public data?: string;
  public username?: string;
  public msgType?: MsgType;
}

class ImgMsg { 
  constructor(data?: SafeUrl, username?: string,msgType?:MsgType) {
    this.data = data;
    this.username = username;
    this.msgType = msgType;
  }

  public data?: SafeUrl;
  public username?: string;
  public msgType?: MsgType;
}

class TextMsg {
  public data?: string;
  public username?: string;
  public msgType?: MsgType;
}
