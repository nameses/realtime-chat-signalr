import { Component, Inject, OnInit } from '@angular/core';
import { AccountService } from '../../services/account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { Route, Router } from '@angular/router';
import { ChatService } from 'src/app/services/signalr.service';

@Component({
  selector: 'app-login-form',
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.css'],
})
export class LoginFormComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  submitted = false;
  error?: string;

  constructor(
    private accountService: AccountService,
    private formBuilder: FormBuilder,
    private router: Router,
    private chatService: ChatService
  ) {}

  ngOnInit() {
    this.form = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';

    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.accountService
      .login(this.f['username'].value, this.f['password'].value)
      .pipe(first())
      .subscribe({
        next: () => {
          console.log('Login successful');
          this.chatService.init();
          this.router.navigateByUrl('');
        },
        error: (error) => {
          this.error = error.error;
          console.log(error);
          this.loading = false;
        },
      });
  }

  openSignUpComponent() {
    this.router.navigateByUrl('register');
  }
}
