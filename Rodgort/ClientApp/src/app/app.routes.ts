import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LogsComponent } from './logs/logs.component';
import { RequestsComponent } from './requests/requests.component';
import { ProgressComponent } from './progress/progress.component';
import { UnknownDeletionResolutionComponent } from './unknown-deletion-resolution/unknown-deletion-resolution.component';
import { ManualQuestionProcessingComponent } from './manual-question-processing/manual-question-processing.component';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent, pathMatch: 'full' },
    { path: 'requests', component: RequestsComponent, pathMatch: 'full' },
    { path: 'logs', component: LogsComponent },
    { path: 'progress', component: ProgressComponent },
    { path: 'unknown-deletion-resolution', component: UnknownDeletionResolutionComponent },
    { path: 'manual-question-processing', component: ManualQuestionProcessingComponent }
];
