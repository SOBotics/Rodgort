import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-tag-bubble',
  templateUrl: './tag-bubble.component.html',
  styleUrls: ['./tag-bubble.component.scss']
})
export class TagBubbleComponent implements OnInit {

  @Input()
  public tag: string;

  constructor() { }

  ngOnInit() {
  }

}
