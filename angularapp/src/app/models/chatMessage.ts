import { MsgType } from './msgtype';

export class ChatMessage {
  public user?: string = '';
  public msgText: string = '';
  public ifPrivate?: boolean;
  public msgType: MsgType = MsgType.Text;
  public userReceiver?: string = '';
}
