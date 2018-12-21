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

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    QuestionCountGraphComponent,
    LogsComponent,
    ArraySortPipe,
    ArraySortAscendingPipe,
    RequestsComponent,
    ProgressComponent
  ],
  imports: [
    ChartModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(appRoutes)
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
