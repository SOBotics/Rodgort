import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PagingInfo } from '../../utils/PagingHelper';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent implements OnInit {

  @Input()
  public pagingInfo: PagingInfo[];

  @Output()
  public pageClicked = new EventEmitter<number>();

  constructor() { }

  ngOnInit() {
  }

  public pageWasClicked(pageNumber: number) {
    this.pageClicked.emit(pageNumber);
  }
}
