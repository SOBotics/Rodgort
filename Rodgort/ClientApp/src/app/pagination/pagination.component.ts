import { Component, OnInit, Input } from '@angular/core';
import { PagingInfo } from '../../utils/PagingHelper';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent implements OnInit {

  @Input()
  public pagingInfo: PagingInfo[];

  @Input()
  public loadPage: (pageNumber: number) => void;

  constructor() { }

  ngOnInit() {
  }

}
