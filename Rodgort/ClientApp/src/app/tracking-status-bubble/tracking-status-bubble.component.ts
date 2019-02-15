import { Component, OnInit, Input } from '@angular/core';
import { tagTrackingStatus } from '../../constants/tag-tracking-status';

@Component({
  selector: 'app-tracking-status-bubble',
  templateUrl: './tracking-status-bubble.component.html',
  styleUrls: ['./tracking-status-bubble.component.scss']
})
export class TrackingStatusBubbleComponent implements OnInit {

  @Input()
  public trackingStatusName: string;

  @Input()
  public trackingStatusId: number;

  tagTrackingStatus = tagTrackingStatus;

  constructor() { }

  ngOnInit() {
  }

}
