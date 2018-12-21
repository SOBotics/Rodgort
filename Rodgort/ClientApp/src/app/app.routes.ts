import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LogsComponent } from './logs/logs.component';
import { RequestsComponent } from './requests/requests.component';
import { ProgressComponent } from './progress/progress.component';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent, pathMatch: 'full' },
    { path: 'requests', component: RequestsComponent, pathMatch: 'full' },
    { path: 'logs', component: LogsComponent },
    { path: 'progress', component: ProgressComponent }
];
