import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Chart } from 'angular-highcharts';
import * as Highcharts from 'highcharts';

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

        const bands = [];
        const lines = [];

        const firstTag = this.burns[0];

        if (firstTag.featuredStarted && firstTag.featuredEnded) {
          bands.push({
            color: 'rgb(251, 237, 182)',
            from: this.toUtcDateTime(firstTag.featuredStarted),
            to: this.toUtcDateTime(firstTag.featuredEnded),
            label: {
              text: 'featured'
            }
          });
        } else if (firstTag.featuredStarted) {
          lines.push({
            color: 'red',
            value: this.toUtcDateTime(firstTag.featuredStarted),
            width: 2,
            label: {
              text: 'featured start'
            }
          });
        } else if (firstTag.featuredEnded) {
          lines.push({
            color: 'red',
            value: this.toUtcDateTime(firstTag.featuredEnded),
            width: 2,
            label: {
              text: 'featured end'
            }
          });
        }

        if (firstTag.burnStarted && firstTag.burnEnded) {
          bands.push({
            color: 'rgb(251, 237, 182)',
            from: this.toUtcDateTime(firstTag.burnStarted),
            to: this.toUtcDateTime(firstTag.burnEnded),
            label: {
              text: 'burnination'
            }
          });
        } else if (firstTag.burnStarted) {
          lines.push({
            color: 'red',
            value: this.toUtcDateTime(firstTag.burnStarted),
            width: 2,
            label: {
              text: 'burn start'
            }
          });
        } else if (firstTag.burnEnded) {
          lines.push({
            color: 'red',
            value: this.toUtcDateTime(firstTag.burnEnded),
            width: 2,
            label: {
              text: 'burn end'
            }
          });
        }

        const minTime = this.toUtcDateTime(
          firstTag.featuredStarted
          || firstTag.featuredEnded
          || firstTag.burnStarted
          || firstTag.burnEnded
        );

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
            type: 'area',
          },
          title: {
            text: ''
          },
          xAxis: {
            type: 'datetime',
            labels: {
              format: '{value:%Y-%m-%d}',
              rotation: 45,
            },
            plotLines: lines,
            plotBands: bands,
            min: minTime,
            tickInterval: 3600 * 24 * 1 * 1000,
          },
          tooltip: {
            formatter: function () {
              return `${Highcharts.dateFormat('%Y-%m-%d %H:%M', this.x)}: ${this.series.name} (${this.y})`;
            }
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
