import { Component, OnInit, Input } from '@angular/core';
import { Chart } from 'angular-highcharts';
import * as Highcharts from 'highcharts';

@Component({
  selector: 'app-question-count-graph',
  templateUrl: './question-count-graph.component.html',
  styleUrls: ['./question-count-graph.component.scss']
})
export class QuestionCountGraphComponent implements OnInit {

  @Input()
  public data: any;
  @Input()
  public closuresOverTime: any;
  @Input()
  public deletionsOverTime: any;
  @Input()
  public retagsOverTime: any;
  @Input()
  public roombasOverTime: any;

  @Input()
  public featuredStarted?: string;
  @Input()
  public featuredEnded?: string;
  @Input()
  public burnStarted?: string;
  @Input()
  public burnEnded?: string;

  public chart: Chart = null;

  constructor() { }

  ngOnInit() {
    const bands = [];
    const lines = [];
    if (this.featuredStarted && this.featuredEnded) {
      bands.push({
        color: 'rgb(251, 237, 182)',
        from: this.toUtcDateTime(this.featuredStarted),
        to: this.toUtcDateTime(this.featuredEnded),
        label: {
          text: 'featured'
        }
      });
    } else if (this.featuredStarted) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.featuredStarted),
        width: 2,
        label: {
          text: 'featured start'
        }
      });
    } else if (this.featuredEnded) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.featuredEnded),
        width: 2,
        label: {
          text: 'featured end'
        }
      });
    }

    if (this.burnStarted && this.burnEnded) {
      bands.push({
        color: 'rgb(251, 189, 182)',
        from: this.toUtcDateTime(this.burnStarted),
        to: this.toUtcDateTime(this.burnEnded),
        label: {
          text: 'burnination'
        }
      });
    } else if (this.burnStarted) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.burnStarted),
        width: 2,
        label: {
          text: 'burn start'
        }
      });
    } else if (this.burnEnded) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.burnEnded),
        width: 2,
        label: {
          text: 'burn end'
        }
      });
    }

    const series: { name: string, data: [number, number][] }[] = [];
    series.push({
      name: 'Total',
      data: this.data.map(gd => {
        const utcDate = this.toUtcDateTime(gd.dateTime);
        return [utcDate, gd.questionCount];
      })
    });

    if (this.closuresOverTime) {
      series.push({
        name: 'Closures',
        data: this.closuresOverTime.map(gd => {
          const utcDate = this.toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }
    if (this.deletionsOverTime) {
      series.push({
        name: 'Deletions',
        data: this.deletionsOverTime.map(gd => {
          const utcDate = this.toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }
    if (this.retagsOverTime) {
      series.push({
        name: 'Retags',
        data: this.retagsOverTime.map(gd => {
          const utcDate = this.toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }
    if (this.roombasOverTime) {
      series.push({
        name: 'Roombas',
        data: this.roombasOverTime.map(gd => {
          const utcDate = this.toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }

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
          rotation: 45,
        },
        plotLines: lines,
        plotBands: bands,
        tickInterval: 3600 * 24 * 1 * 1000,
      },
      tooltip: {
        formatter: function () {
          const actionType =
            this.series.name === 'Total'
              ? 'seen on'
              : this.series.name === 'Closures'
                ? 'closed by'
                : this.series.name === 'Deletions'
                  ? 'deleted by'
                  : this.series.name === 'Retags'
                    ? 'retagged by'
                    : this.series.name === 'Roombas'
                      ? 'roomba\'d by'
                      : null;

          if (actionType) {
            return `${this.y} questions ${actionType} ${Highcharts.dateFormat('%Y-%m-%d %H:%M', this.x)}`;
          }
          return '';
        }
      },
      yAxis: {
        title: {
          text: 'Total questions'
        },
        min: 0
      },
      credits: {
        enabled: false
      },
      legend: {
        enabled: this.closuresOverTime || this.deletionsOverTime || this.retagsOverTime || this.roombasOverTime
      },
      series: series
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
