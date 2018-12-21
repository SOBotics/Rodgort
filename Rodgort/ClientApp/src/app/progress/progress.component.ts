import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Chart } from 'angular-highcharts';

@Component({
  selector: 'app-progress',
  templateUrl: './progress.component.html',
  styleUrls: ['./progress.component.scss']
})
export class ProgressComponent implements OnInit {
  public burns: any;
  public chart: Chart;

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    this.httpClient.get('/api/statistics/leaderboard/current')
      .subscribe((data: any) => {
        this.burns = data.burns;

        const series: { name: string, data: [number, number][] }[] = this.burns[0].tags[0].overtime.map(o => {
          return {
            name: o.user + ' - ' + o.type,
            data: o.times.map((gd: any) => {
              const utcDate = this.toUtcDateTime(gd.date);
              return [utcDate, gd.total];
            })
          };
        });

        this.chart = new Chart({
          chart: {
            type: 'line',
          },
          title: {
            text: ''
          },
          xAxis: {
            type: 'datetime',
            labels: {
              format: '{value:%Y-%m-%d}',
              rotation: 45
            },
          },
          yAxis: {
            title: {
              text: 'Actions'
            },
            min: 0
          },
          credits: {
            enabled: false
          },
          legend: {
            enabled: false
          },
          series: series
        });
      });
  }

  private toUtcDateTime(num: string): number {
    const date = new Date(num);
    const utcDate = Date.UTC(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      date.getHours(),
      date.getMinutes(),
      date.getSeconds()
    );
    return utcDate;
  }
}
