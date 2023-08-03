import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private openLoginSource = new Subject<void>();
  private openSignUpSource = new Subject<void>();

  openLogin$ = this.openLoginSource.asObservable();
  openSignUp$ = this.openSignUpSource.asObservable();

  private closeModalSource = new Subject<void>();
  closeModal$ = this.closeModalSource.asObservable();

  openLogin() {
    this.openLoginSource.next();
  }

  openSignUp() {
    this.openSignUpSource.next();
  }

  closeComponent() {
    this.closeModalSource.next();
  }
}
