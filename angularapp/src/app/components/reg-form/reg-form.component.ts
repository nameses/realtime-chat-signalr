import { Component } from '@angular/core';
import { ModalService } from '../../services/modal.service';
import { AccountService } from 'src/app/services/account.service';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { first } from 'rxjs/operators';
import CustomValidators from 'src/utils/custom-validators';

@Component({
  selector: 'app-reg-form',
  templateUrl: './reg-form.component.html',
  styleUrls: ['./reg-form.component.css'],
})
export class RegFormComponent {
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
    this.form = this.formBuilder.group(
      {
        username: ['', Validators.required],
        password: ['', Validators.required],
        confirmPassword: ['', Validators.required],
      },
      {
        validators: [CustomValidators.match('password', 'confirmPassword')],
      }
    );
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
      .register(this.f['username'].value, this.f['password'].value)
      .pipe(first())
      .subscribe({
        next: () => {
          this.openLoginComponent();
          console.log('Sign up successful');
        },
        error: (error) => {
          this.error = error.error;
          console.log(error);
          this.loading = false;
        },
      });
  }

  openLoginComponent() {
    this.modalService.openLogin();
  }

  closeComponent() {
    console.log('Trying to close sign up component.');
    this.modalService.closeComponent();
  }
}
