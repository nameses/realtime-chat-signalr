import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { User } from '../models/user';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private userSubject: BehaviorSubject<User | null>;
  public user: Observable<User | null>;

  constructor(
    private http: HttpClient
  ) {
    this.userSubject = new BehaviorSubject(JSON.parse(localStorage.getItem('user')!));
    this.user = this.userSubject.asObservable();
  }

  public get userValue() {
    return this.userSubject.value;
  }

  register(username:string, password:string){
    return this.http.post<User>(`${environment.apiUrl}/auth/register`, {username,password})
      .pipe(map(user=>{
        console.log(`Successfully registered user.`)
        return user;
      }));
  }

  login(username: string, password: string) {
    return this.http.post<User>(`${environment.apiUrl}/auth/login`, { username, password }, { observe: 'response' })
      .pipe(map(response  => {
        const user = response.body;
        console.log(user);
        //user data
        localStorage.setItem('user', JSON.stringify(user));

        this.userSubject.next(user);
        return user;
      }));
  }

  logout() {
    // remove user from local storage and set current user to null
    localStorage.removeItem('user');
    this.userSubject.next(null);
    window.location.reload();
  }
}
