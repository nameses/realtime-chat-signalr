export class UserConnection {
  username: string = '';
  connectionId: string = '';

  /**
   *
   */
  constructor(username: string, connectionId: string) {
    this.username = username;
    this.connectionId = connectionId;
  }
}
