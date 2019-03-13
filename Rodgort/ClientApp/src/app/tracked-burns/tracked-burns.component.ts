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
    this.httpClient.get('/api/statistics/trackedBurns').subscribe((burns: any[]) => {
      this.burns = burns.map(burn => {
        const momBurnStarted = moment.utc(burn.burnStarted);
        const momBurnEnded = burn.burnEnded ? moment.utc(burn.burnEnded) : undefined;

        let durationStr: string | undefined;
        if (momBurnEnded) {
          let duration = moment.duration(momBurnEnded.diff(momBurnStarted));
          const days = duration.asDays();
          duration = duration.subtract(days, 'days');
          const hours = duration.hours();
          duration = duration.subtract(hours, 'hours');
          const minutes = duration.minutes();
          durationStr = days > 0
            ? `${days} day${days > 1 ? 's' : ''}, ${hours} hour${hours > 1 ? 's' : ''} and ${minutes} minute${minutes > 1 ? 's' : ''}`
            : hours > 0 ? `${hours} hour${hours > 1 ? 's' : ''} and ${minutes} minute${minutes > 1 ? 's' : ''}`
              : `${minutes} minute${minutes > 1 ? 's' : ''}`;
        }
        const result = {
          ...burn,
          burnStartedLocal: momBurnStarted.local().format('YYYY-MM-DD hh:mm:ss A'),
          burnEndedLocal: momBurnEnded ? momBurnEnded.local().format('YYYY-MM-DD hh:mm:ss A') : undefined,
          duration: durationStr
        };
        return result;
      });
      console.log(this.burns);
      this.loading = false;
    });
  }

}
