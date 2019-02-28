import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import { isArray } from 'util';

export const AccessTokenStorageKey = 'access_token';
export interface Claim { key: string; value: any; }
export interface AuthDetails {
  RawToken: string;
  TokenData: any;
  IsAuthenticated: boolean;
  Claims: Claim[];
  HasRole: (role: number) => any;
}

const ROLE_TYPE = 'role';

export const RODGORT_SUPER_USER = 1;
export const RODGORT_ADMIN = 2;

@Injectable()
export class AuthService {
  public RawToken: string;

  private subject: BehaviorSubject<AuthDetails> = new BehaviorSubject<AuthDetails>(this.getAuthDetails());
  constructor() {
  }

  public GetAuthDetails(): Observable<AuthDetails> {
    return this.subject;
  }

  private getAuthDetails(): AuthDetails {
    const accessToken = localStorage.getItem(AccessTokenStorageKey);
    let payload = null;
    if (accessToken) {
      const tokenPayload = accessToken.split('.')[1];
      payload = JSON.parse(atob(tokenPayload));
      payload = this.HandleExpiryDates(payload);
    }

    const claims: Claim[] = [];
    if (payload) {
      const keys = Object.keys(payload);
      keys.forEach(key => {
        const value = payload[key];
        if (isArray(value)) {
          for (const arrValue of value) {
            claims.push({ key: key, value: arrValue });
          }
        } else {
          claims.push({ key: key, value: payload[key] });
        }
      });
    }
    const hasRole = (role: number) => {
      const matchingClaim = claims.find(a => a.key === ROLE_TYPE && a.value === (role + ''));
      return !!matchingClaim;
    };

    return {
      RawToken: accessToken,
      TokenData: payload,
      IsAuthenticated: !!payload,
      Claims: claims,
      HasRole: hasRole
    };
  }

  private HandleExpiryDates(payload: any) {
    const now = new Date();
    if (!payload || !payload.exp) {
      return undefined;
    } else {
      const expDate = new Date(payload.exp * 1000);
      // Typescript doesn't recognize that dates can be subtracted
      let timeToRefresh = expDate as any - (now as any);
      timeToRefresh -= (1000 * 60 * 5); // Refresh 5 minutes before expiry
      if (timeToRefresh < 0) {
        return undefined;
      }
      //   setTimeout(() => {
      //     this.authenticationService.authenticationRefreshTokenPost().subscribe(token => {
      //       this.Login(token);
      //     });
      //   }, timeToRefresh);
      return payload;
    }
  }

  public Login(token: string) {
    localStorage.setItem(AccessTokenStorageKey, token);
    this.subject.next(this.getAuthDetails());
  }
  public Logout() {
    localStorage.removeItem(AccessTokenStorageKey);
    document.cookie = 'access_token=; Path=/Hangfire; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    this.subject.next(this.getAuthDetails());
  }
}
