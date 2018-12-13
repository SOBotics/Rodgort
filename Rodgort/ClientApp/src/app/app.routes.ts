import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LogsComponent } from './logs/logs.component';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent, pathMatch: 'full' },
    { path: 'logs', component: LogsComponent }
];
