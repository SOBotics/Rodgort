import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LogsComponent } from './logs/logs.component';
import { RequestsComponent } from './requests/requests.component';
import { ProgressComponent } from './progress/progress.component';
import { UnknownDeletionResolutionComponent } from './unknown-deletion-resolution/unknown-deletion-resolution.component';
import { ManualQuestionProcessingComponent } from './manual-question-processing/manual-question-processing.component';
import { TagTrackingStatusAuditsComponent } from './tag-tracking-status-audits/tag-tracking-status-audits.component';
import { UserComponent } from './user/user.component';
import { ZombiesComponent } from './zombies/zombies.component';
import { UsersComponent } from './users/users.component';
import { TrackedBurnsComponent } from './tracked-burns/tracked-burns.component';
import { UserActionsComponent } from './user-actions/user-actions.component';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent, pathMatch: 'full' },
    { path: 'requests', component: RequestsComponent, pathMatch: 'full' },
    { path: 'logs', component: LogsComponent },
    { path: 'progress/:type', component: ProgressComponent },
    { path: 'progress', component: ProgressComponent },
    { path: 'tag-tracking-status-audits', component: TagTrackingStatusAuditsComponent },
    { path: 'unknown-deletion-resolution', component: UnknownDeletionResolutionComponent },
    { path: 'manual-question-processing', component: ManualQuestionProcessingComponent },
    { path: 'user/:id', component: UserComponent },
    { path: 'user-actions/:id', component: UserActionsComponent },
    { path: 'profile', component: UserComponent },
    { path: 'zombies', component: ZombiesComponent },
    { path: 'users', component: UsersComponent },
    { path: 'tracked-burns', component: TrackedBurnsComponent }
];
