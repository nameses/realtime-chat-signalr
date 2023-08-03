import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppComponent } from './app.component';
import { JwtInterceptor } from '../utils/jwt-interceptor';
import { LoginFormComponent } from './components/login-form/login-form.component'
import { MatDialogModule } from '@angular/material/dialog';
import { RegFormComponent } from './components/reg-form/reg-form.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@NgModule({
  declarations: [AppComponent, LoginFormComponent, RegFormComponent],
  imports: [BrowserModule, HttpClientModule, FormsModule, MatDialogModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  providers: [{ provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },],
  bootstrap: [AppComponent],
})
export class AppModule {}
