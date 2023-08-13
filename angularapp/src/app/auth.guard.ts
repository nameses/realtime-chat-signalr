import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { AccountService } from './services/account.service';
import { Injectable, inject } from '@angular/core';

export const canActivate: CanActivateFn = (route, state) => {
  return inject(PermissionsService).canActivate();
};
export const canActivateLogin: CanActivateFn = (route, state) => {
  return inject(PermissionsService).canActivateLogin();
};

@Injectable({
  providedIn: 'root',
})
class PermissionsService {
  constructor(private accountService: AccountService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    return this.accountService.isLoggedIn
      ? true
      : this.router.parseUrl('login');
  }
  canActivateLogin(): boolean | UrlTree {
    return this.accountService.isLoggedIn ? this.router.parseUrl('') : true;
  }
}
