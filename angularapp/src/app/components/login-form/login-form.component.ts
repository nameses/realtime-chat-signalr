import { Component, Inject, OnInit } from '@angular/core';
import { ModalService } from '../../services/modal.service';
import { AccountService } from '../../services/account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';

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
    private modalService: ModalService,
    private accountService: AccountService,
    private formBuilder: FormBuilder
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
          this.closeComponent();
          console.log('Login successful:');
        },
        error: (error) => {
          this.error = error.error;
          console.log(error);
          this.loading = false;
        },
      });
  }

  openSignUpComponent() {
    this.modalService.openSignUp();
  }

  closeComponent() {
    console.log('Trying to close login component.');
    this.modalService.closeComponent();
  }
}
