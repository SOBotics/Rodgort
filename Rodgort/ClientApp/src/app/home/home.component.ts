import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  public isAdmin = true;
  public loading = false;
  public pagingInfo: PagingInfo[];
  public filter = {
    tag: '',
    type: -1,
    approvalStatus: -1,
    status: '',
    pageNumber: 1,
    sortBy: 'score'
  };

  public questions: any[];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.tag = params['tag'] || '';
      this.filter.type = +params['type'] || -1;
      this.filter.approvalStatus = +params['approvalStatus'] || -1;
      this.filter.status = params['status'] || '';
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.filter.sortBy = params['sortBy'] || 'score';
      this.reloadData();
    });
  }

  public reloadData() {
    const query =
      `/api/MetaQuestions` +
      `?tag=${this.filter.tag}` +
      `&approvalStatus=${this.filter.approvalStatus}` +
      `&type=${this.filter.type}` +
      `&status=${this.filter.status}` +
      `&sortBy=${this.filter.sortBy}` +
      `&page=${this.filter.pageNumber}` +
      `&pageSize=10`;

    // this.loading = true;
    this.httpClient.get(query)
      .subscribe((response: any) => {
        this.loading = false;
        if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
          this.filter.pageNumber = 1;
          this.reloadData();
        } else {
          this.questions = response.data;
          this.pagingInfo = GetPagingInfo(response);
        }
      });
  }

  public setApprovalStatus(metaQuestionId: number, tagName: string, approved: boolean) {
    this.httpClient.post('/api/MetaQuestions/SetTagApprovalStatus', {
      metaQuestionId,
      tagName,
      approved
    }).subscribe(_ => {
      this.reloadData();
    });
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.applyFilter();
  }

  public applyFilter() {
    this.router.navigate(['/'], { queryParams: this.filter });
  }
}
