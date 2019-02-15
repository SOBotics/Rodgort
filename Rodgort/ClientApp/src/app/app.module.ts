import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { QuestionCountGraphComponent } from './question-count-graph/question-count-graph.component';
import { ChartModule } from 'angular-highcharts';
import { appRoutes } from './app.routes';
import { LogsComponent } from './logs/logs.component';
import { ArraySortPipe } from '../pipes/ArraySortPipe';
import { RequestsComponent } from './requests/requests.component';
import { ProgressComponent } from './progress/progress.component';
import { ArraySortAscendingPipe } from '../pipes/ArraySortAscendingPipe';
import { AuthService } from './services/auth.service';
import { UnknownDeletionResolutionComponent } from './unknown-deletion-resolution/unknown-deletion-resolution.component';
import { ManualQuestionProcessingComponent } from './manual-question-processing/manual-question-processing.component';
import { PaginationComponent } from './pagination/pagination.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSnackBarModule } from '@angular/material';
import { NewVersionDeployingComponent } from './snackbar/new-version-deploying/new-version-deploying.component';
import { TagTrackingStatusAuditsComponent } from './tag-tracking-status-audits/tag-tracking-status-audits.component';
import { UserComponent } from './user/user.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    QuestionCountGraphComponent,
    LogsComponent,
    ArraySortPipe,
    ArraySortAscendingPipe,
    RequestsComponent,
    ProgressComponent,
    UnknownDeletionResolutionComponent,
    ManualQuestionProcessingComponent,
    PaginationComponent,
    NewVersionDeployingComponent,
    TagTrackingStatusAuditsComponent,
    UserComponent
  ],
  imports: [
    ChartModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(appRoutes),
    BrowserAnimationsModule,
    MatSnackBarModule
  ],
  providers: [
    AuthService
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    NewVersionDeployingComponent
  ]
})
export class AppModule {
}
