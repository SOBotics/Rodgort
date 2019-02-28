import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Chart } from 'angular-highcharts';
import * as Highcharts from 'highcharts';
import { AuthService, ROLE_MODERATOR } from '../services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { toUtcDateTime } from '../../utils/ToUtcDateTime';

@Component({
  selector: 'app-progress',
  templateUrl: './progress.component.html',
  styleUrls: ['./progress.component.scss']
})
export class ProgressComponent implements OnInit {
  public loading = true;
  public hasNoData = false;

  public isModerator = false;

  public burns: any;

  public userGrandTotalShowAll = false;
  public userBreakdownShowAll = false;

  public filter = {
    metaQuestionId: -1,
    all: false,
  };

  constructor(
    private httpClient: HttpClient,
    private route: ActivatedRoute,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.authService.GetAuthDetails().subscribe(d => {
      this.isModerator = d.HasRole(ROLE_MODERATOR);
    });

    let isQueryParamsFirstUpdate = true;
    this.route.queryParams.subscribe(params => {
      this.filter.metaQuestionId = +params['metaQuestionId'] || -1;
      if (!isQueryParamsFirstUpdate) {
        this.reloadData();
      }
      isQueryParamsFirstUpdate = false;
    });

    let isParamsFirstUpdate = true;
    this.route.params.subscribe(params => {
      this.filter.all = (params['type'] !== undefined);
      if (!isParamsFirstUpdate) {
        this.reloadData();
      }
      isParamsFirstUpdate = false;
    });
    this.reloadData();
  }

  private reloadData() {
    this.loading = true;
    this.hasNoData = false;
    const endpoint =
      this.filter.metaQuestionId >= 0 ?
        `/api/statistics/leaderboard?metaQuestionId=${this.filter.metaQuestionId}`
        : this.filter.all
          ? '/api/statistics/leaderboard/all'
          : '/api/statistics/leaderboard/current';

    this.httpClient.get(endpoint)
      .subscribe((data: any) => {
        this.loading = false;

        this.burns = data.burns;

        if (this.burns.length === 0 || this.burns[0].tags[0].length === 0) {
          this.hasNoData = true;
          return;
        }

        for (const burn of this.burns) {
          const bands = [];
          const lines = [];

          if (burn.featuredStarted && burn.featuredEnded) {
            bands.push({
              color: 'rgb(251, 237, 182)',
              from: toUtcDateTime(burn.featuredStarted),
              to: toUtcDateTime(burn.featuredEnded),
              label: {
                text: 'featured'
              }
            });
          } else if (burn.featuredStarted) {
            lines.push({
              color: 'red',
              value: toUtcDateTime(burn.featuredStarted),
              width: 2,
              label: {
                text: 'featured start'
              }
            });
          } else if (burn.featuredEnded) {
            lines.push({
              color: 'red',
              value: toUtcDateTime(burn.featuredEnded),
              width: 2,
              label: {
                text: 'featured end'
              }
            });
          }

          if (burn.burnStarted && burn.burnEnded) {
            bands.push({
              color: 'rgb(251, 189, 182)',
              from: toUtcDateTime(burn.burnStarted),
              to: toUtcDateTime(burn.burnEnded),
              label: {
                text: 'burnination'
              }
            });
          } else if (burn.burnStarted) {
            lines.push({
              color: 'red',
              value: toUtcDateTime(burn.burnStarted),
              width: 2,
              label: {
                text: 'burn start'
              }
            });
          } else if (burn.burnEnded) {
            lines.push({
              color: 'red',
              value: toUtcDateTime(burn.burnEnded),
              width: 2,
              label: {
                text: 'burn end'
              }
            });
          }

          const minTime = toUtcDateTime(
            burn.featuredStarted
            || burn.featuredEnded
            || burn.burnStarted
            || burn.burnEnded
          );

          for (const tag of burn.tags) {
            const series: { name: string, data: [number, number][] }[] = tag.overtime.map(o => {
              return {
                name: o.userName + (o.isModerator ? ' ♦' : ''),
                data: o.times.map((gd: any) => {
                  const utcDate = toUtcDateTime(gd.date);
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
              plotOptions: {
                area: {
                  stacking: 'normal',
                  marker: {
                    enabled: false
                  }
                }
              },
              credits: {
                enabled: false
              },
              series: series
            });
          }
        }
      });
  }
}
