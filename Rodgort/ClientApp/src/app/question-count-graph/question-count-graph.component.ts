import { Component, OnInit, Input } from '@angular/core';
import { Chart } from 'angular-highcharts';
import * as Highcharts from 'highcharts';
import { toUtcDateTime } from '../../utils/ToUtcDateTime';

@Component({
  selector: 'app-question-count-graph',
  templateUrl: './question-count-graph.component.html',
  styleUrls: ['./question-count-graph.component.scss']
})
export class QuestionCountGraphComponent implements OnInit {

  @Input()
  public data: any;
  @Input()
  public remainingOverTime: any;
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
  @Input()
  public customLines?: { lineName?: string; position: any; width?: number; colour: string; }[];

  public chart: Chart = null;

  constructor() { }

  ngOnInit() {
    const bands = [];
    const lines = this.customLines ?
      this.customLines.map(l => ({
        color: l.colour,
        value: l.position,
        width: l.width || 1,
        label: {
          text: l.lineName
        }
      }))
      : [];

    if (this.featuredStarted && this.featuredEnded) {
      bands.push({
        color: 'rgb(251, 237, 182)',
        from: toUtcDateTime(this.featuredStarted),
        to: toUtcDateTime(this.featuredEnded),
        label: {
          text: 'featured'
        }
      });
    } else if (this.featuredStarted) {
      lines.push({
        color: 'red',
        value: toUtcDateTime(this.featuredStarted),
        width: 2,
        label: {
          text: 'featured start'
        }
      });
    } else if (this.featuredEnded) {
      lines.push({
        color: 'red',
        value: toUtcDateTime(this.featuredEnded),
        width: 2,
        label: {
          text: 'featured end'
        }
      });
    }

    if (this.burnStarted && this.burnEnded) {
      bands.push({
        color: 'rgb(251, 189, 182)',
        from: toUtcDateTime(this.burnStarted),
        to: toUtcDateTime(this.burnEnded),
        label: {
          text: 'burnination'
        }
      });
    } else if (this.burnStarted) {
      lines.push({
        color: 'red',
        value: toUtcDateTime(this.burnStarted),
        width: 2,
        label: {
          text: 'burn start'
        }
      });
    } else if (this.burnEnded) {
      lines.push({
        color: 'red',
        value: toUtcDateTime(this.burnEnded),
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
        const utcDate = toUtcDateTime(gd.dateTime);
        return [utcDate, gd.questionCount];
      })
    });

    if (this.remainingOverTime) {
      series.push({
        name: 'Remaining',
        data: this.remainingOverTime.map(gd => {
          const utcDate = toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }

    if (this.closuresOverTime) {
      series.push({
        name: 'Closures',
        data: this.closuresOverTime.map(gd => {
          const utcDate = toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }
    if (this.deletionsOverTime) {
      series.push({
        name: 'Deletions',
        data: this.deletionsOverTime.map(gd => {
          const utcDate = toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }
    if (this.retagsOverTime) {
      series.push({
        name: 'Retags',
        data: this.retagsOverTime.map(gd => {
          const utcDate = toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }
    if (this.roombasOverTime) {
      series.push({
        name: 'Roombas',
        data: this.roombasOverTime.map(gd => {
          const utcDate = toUtcDateTime(gd.date);
          return [utcDate, gd.total];
        })
      });
    }

    const minTime = toUtcDateTime(
      this.featuredStarted
      || this.featuredEnded
      || this.burnStarted
      || this.burnEnded
    );

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
          rotation: -45,
        },
        plotLines: lines,
        plotBands: bands,
        min: minTime,
        tickInterval: 3600 * 24 * 1 * 1000,
      },
      plotOptions: {
        line: {
          marker: {
            enabled: false
          }
        }
      },
      tooltip: {
        formatter: function () {
          const actionType =
            this.series.name === 'Total'
              ? 'seen on'
              : this.series.name === 'Remaining'
                ? 'remaining'
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
        enabled: this.remainingOverTime || this.closuresOverTime || this.deletionsOverTime || this.retagsOverTime || this.roombasOverTime
      },
      series: series
    });
  }
}
