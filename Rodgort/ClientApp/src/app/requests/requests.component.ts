import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-requests',
  templateUrl: './requests.component.html',
  styleUrls: ['./requests.component.scss'],
})
export class RequestsComponent implements OnInit {
  public isAdmin = true;
  public loading = false;
  public pagingInfo: PagingInfo[];
  public showRejectedTags: boolean;

  public filter = {
    tag: '',
    type: -1,
    approvalStatus: -1,
    status: '',
    pageNumber: 1,
    hasQuestions: 'true',
    sortBy: 'score'
  };

  model = { options: '2' };

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
      this.filter.hasQuestions = params['hasQuestions'] || 'any';
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
      `&hasQuestions=${this.filter.hasQuestions}` +
      `&sortBy=${this.filter.sortBy}` +
      `&page=${this.filter.pageNumber}` +
      `&pageSize=10`;

    this.loading = true;
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

  public setApprovalStatus(metaQuestionId: number, tag: any, approved: boolean) {
    this.httpClient.post('/api/MetaQuestions/SetTagApprovalStatus', {
      metaQuestionId,
      tagName: tag.tagName,
      approved
    }).subscribe(_ => {
      tag.status = approved ? 'Approved' : 'Rejected';
    });
  }

  public setRequestType(metaQuestionId: number, tag: any, requestType: string) {
    this.httpClient.post('/api/MetaQuestions/SetTagRequestType', {
      metaQuestionId,
      tagName: tag.tagName,
      requestType
    }).subscribe(_ => {
      tag.type = requestType;
    });
  }

  public onNewTagAdded(question: any, event: any) {
    if (event.key === 'Enter') {
      event.preventDefault();
      const newTagName = event.target.value;

      const matchedTag = question.mainTags.find((mt: any) => mt.tagName === newTagName);

      if (matchedTag && matchedTag.status !== 'Rejected') {
        return;
      }

      event.target.value = '';

      this.httpClient.post('/api/MetaQuestions/AddTag', {
        metaQuestionId: question.id,
        tagName: newTagName,
        requestType: 'Burninate'
      }).subscribe(_ => {
        if (matchedTag) {
          matchedTag.status = 'Approved';
          matchedTag.statusId = 2;
        } else {
          question.mainTags = question.mainTags.concat([{
            questionCountOverTime: [],
            status: 'Approved',
            statusId: 2,
            tagName: newTagName,
            type: 'Burninate'
          }]);
        }
      });
    }
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.applyFilter();
  }

  public applyFilter() {
    this.router.navigate(['/requests'], { queryParams: this.filter });
  }

  public setHasQuestions(newValue: boolean) {
    // this.filter.hasQuestions = newValue;
    console.log(newValue);
  }
}
