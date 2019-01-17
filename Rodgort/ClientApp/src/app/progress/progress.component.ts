import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Chart } from 'angular-highcharts';
import * as Highcharts from 'highcharts';
import { AuthService } from '../services/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-progress',
  templateUrl: './progress.component.html',
  styleUrls: ['./progress.component.scss']
})
export class ProgressComponent implements OnInit {
  public loading = true;
  public hasNoData = false;

  public burns: any;

  public filter = {
    metaQuestionId: -1
  };

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.metaQuestionId = +params['metaQuestionId'] || -1;
      this.reloadData();
    });
  }

  private reloadData() {
    this.loading = true;
    this.hasNoData = false;
    this.authService.GetAuthDetails().subscribe(d => {
      const endpoint = this.filter.metaQuestionId >= 0 ?
        `/api/statistics/leaderboard?metaQuestionId=${this.filter.metaQuestionId}`
        : '/api/statistics/leaderboard/current';

      this.httpClient.get(endpoint, {
        headers: { 'Authorization': 'Bearer ' + d.RawToken }
      })
        .subscribe((data: any) => {
          this.loading = false;

          this.burns = data.burns;

          const bands = [];
          const lines = [];

          if (this.burns.length === 0 || this.burns[0].tags[0].length === 0) {
            this.hasNoData = true;
            return;
          }

          for (const burn of this.burns) {
            for (const tag of burn.tags) {
              if (tag.featuredStarted && tag.featuredEnded) {
                bands.push({
                  color: 'rgb(251, 237, 182)',
                  from: this.toUtcDateTime(tag.featuredStarted),
                  to: this.toUtcDateTime(tag.featuredEnded),
                  label: {
                    text: 'featured'
                  }
                });
              } else if (tag.featuredStarted) {
                lines.push({
                  color: 'red',
                  value: this.toUtcDateTime(tag.featuredStarted),
                  width: 2,
                  label: {
                    text: 'featured start'
                  }
                });
              } else if (tag.featuredEnded) {
                lines.push({
                  color: 'red',
                  value: this.toUtcDateTime(tag.featuredEnded),
                  width: 2,
                  label: {
                    text: 'featured end'
                  }
                });
              }

              if (tag.burnStarted && tag.burnEnded) {
                bands.push({
                  color: 'rgb(251, 189, 182)',
                  from: this.toUtcDateTime(tag.burnStarted),
                  to: this.toUtcDateTime(tag.burnEnded),
                  label: {
                    text: 'burnination'
                  }
                });
              } else if (tag.burnStarted) {
                lines.push({
                  color: 'red',
                  value: this.toUtcDateTime(tag.burnStarted),
                  width: 2,
                  label: {
                    text: 'burn start'
                  }
                });
              } else if (tag.burnEnded) {
                lines.push({
                  color: 'red',
                  value: this.toUtcDateTime(tag.burnEnded),
                  width: 2,
                  label: {
                    text: 'burn end'
                  }
                });
              }

              const minTime = this.toUtcDateTime(
                tag.featuredStarted
                || tag.featuredEnded
                || tag.burnStarted
                || tag.burnEnded
              );

              const series: { name: string, data: [number, number][] }[] = tag.overtime.map(o => {
                return {
                  name: o.user,
                  data: o.times.map((gd: any) => {
                    const utcDate = this.toUtcDateTime(gd.date);
                    return [utcDate, gd.total];
                  })
                };
              });

              if (!series.length) {
                return;
              }

              tag.chart = new Chart({
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
            }
          }
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
