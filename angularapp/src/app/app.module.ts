import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppComponent } from './app.component';
import { JwtInterceptor } from '../utils/jwt-interceptor';
import { LoginFormComponent } from './components/login-form/login-form.component';
import { MatDialogModule } from '@angular/material/dialog';
import { RegFormComponent } from './components/reg-form/reg-form.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { GlobalChatComponent } from './components/global-chat/global-chat.component';

import { Routes, RouterModule } from '@angular/router';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { HomeComponent } from './components/home/home.component';
import { UsersListComponent } from './components/users-list/users-list.component';
import { PrivateChatComponent } from './components/private-chat/private-chat.component';
import { canActivate, canActivateLogin } from './auth.guard';

const appRoutes: Routes = [
  { path: '', component: HomeComponent, canActivate: [canActivate] },
  {
    path: 'login',
    component: LoginFormComponent,
    canActivate: [canActivateLogin],
  },
  {
    path: 'register',
    component: RegFormComponent,
    canActivate: [canActivateLogin],
  },
  {
    path: 'global-chat',
    component: GlobalChatComponent,
    canActivate: [canActivate],
  },
  {
    path: 'users-list',
    component: UsersListComponent,
    canActivate: [canActivate],
  },
  { path: '**', component: NotFoundComponent },
];

@NgModule({
  declarations: [
    AppComponent,
    LoginFormComponent,
    RegFormComponent,
    GlobalChatComponent,
    NotFoundComponent,
    HomeComponent,
    UsersListComponent,
    PrivateChatComponent,
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot(appRoutes),
    HttpClientModule,
    FormsModule,
    MatDialogModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
