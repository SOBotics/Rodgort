import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
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
import { TagTrackingStatusAuditsComponent } from './tag-tracking-status-audits/tag-tracking-status-audits.component';
import { UserComponent } from './user/user.component';
import { TagBubbleComponent } from './tag-bubble/tag-bubble.component';
import { TrackingStatusBubbleComponent } from './tracking-status-bubble/tracking-status-bubble.component';
import {ToasterModule } from 'angular2-toaster';
import { HttpErrorInterceptor } from './interceptors/HttpErrorInterceptor';
import { HttpAuthenticationInterceptor } from './interceptors/HttpAuthenticationInterceptor';
import { ZombiesComponent } from './zombies/zombies.component';
import { UsersComponent } from './users/users.component';
import { TrackedBurnsComponent } from './tracked-burns/tracked-burns.component';
import { RenderUsernameComponent } from './render-username/render-username.component';
import { UserActionsComponent } from './user-actions/user-actions.component';

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
    TagTrackingStatusAuditsComponent,
    UserComponent,
    TagBubbleComponent,
    TrackingStatusBubbleComponent,
    ZombiesComponent,
    UsersComponent,
    TrackedBurnsComponent,
    RenderUsernameComponent,
    UserActionsComponent
  ],
  imports: [
    ChartModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(appRoutes),
    BrowserAnimationsModule,
    ToasterModule.forRoot()
  ],
  providers: [
    AuthService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpAuthenticationInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpErrorInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
