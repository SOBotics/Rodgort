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
  public thing: any;

  public chart: Chart = null;

  constructor() { }

  ngOnInit() {
    const series: { name: string, data: [number, number][] }[] = [];
    series.push({
      name: '',
      data: this.thing.map(gd => {
        const date = new Date(gd.dateTime);
        const utcDate = Date.UTC(
          date.getFullYear(),
          date.getMonth(),
          date.getDate(),
          date.getHours(),
          date.getMinutes(),
          date.getSeconds()
        );
        return [utcDate, gd.questionCount];
      })
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
          format: '{value:%Y-%m-%d}'
        }
      },
      tooltip: {
        formatter: function () {
          return `${this.y} questions seen on ${Highcharts.dateFormat('%Y-%m-%d', this.x)}`;
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
        enabled: false
      },
      series: series
    });
  }

}
