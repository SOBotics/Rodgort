import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as moment from 'moment';

@Component({
  selector: 'app-tracked-burns',
  templateUrl: './tracked-burns.component.html',
  styleUrls: ['./tracked-burns.component.scss']
})
export class TrackedBurnsComponent implements OnInit {
  public burns: any;
  public loading = false;
  constructor(
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.loading = true;
    this.httpClient.get('/api/statistics/trackedBurns').subscribe((response: any[]) => {
      this.burns = response.map(d => ({
        ...d,
        burnStartedLocal: moment.utc(d.burnStarted).local().format('YYYY-MM-DD hh:mm:ss A'),
        burnEndedLocal: d.burnEnded ? moment.utc(d.burnEnded).local().format('YYYY-MM-DD hh:mm:ss A') : undefined,
      }));
      this.loading = false;
    });
  }

}
