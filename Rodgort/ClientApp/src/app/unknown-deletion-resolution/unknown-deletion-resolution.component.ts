import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService, AuthDetails } from '../services/auth.service';

@Component({
  selector: 'app-unknown-deletion-resolution',
  templateUrl: './unknown-deletion-resolution.component.html',
  styleUrls: ['./unknown-deletion-resolution.component.scss']
})
export class UnknownDeletionResolutionComponent implements OnInit {
  public getRevision: (postId: number) => Promise<XMLHttpRequest>;

  public postsToVisit = [];

  private userIdRegex = /\/users\/(\-?\d+)/g;

  private authDetails: AuthDetails;

  constructor(private httpClient: HttpClient, private authService: AuthService) { }

  ngOnInit() {
    const checkerInterval = setInterval(() => {
      const untypedWindow = window as any;
      if (untypedWindow.getRevision) {
        this.getRevision = untypedWindow.getRevision;
        clearInterval(checkerInterval);

        this.Setup();
      }
    }, 500);
  }

  private Setup() {
    this.authService.GetAuthDetails().subscribe(authDetails => {
      this.authDetails = authDetails;

      this.httpClient.get('/api/admin/UnresolvedDeletions', {
        headers: { 'Authorization': 'Bearer ' + authDetails.RawToken }
      })
        .subscribe((data: any) => {
          this.postsToVisit = data.map(action => ({
            unknownDeletionId: action.unknownDeletionId,
            postId: action.postId,
            info: '',
            status: 'pending'
          }));
        });
    });
  }

  public startProcessing() {
    let i = 0;
    const processNext = () => {
      if (i > this.postsToVisit.length - 1) {
        return;
      }

      const postToVisit = this.postsToVisit[i];
      i++;
      postToVisit.status = 'processing';
      const revisionInfoPromise = this.getRevision(postToVisit.postId);
      revisionInfoPromise.then(revisionInfo => {
        const container = document.implementation.createHTMLDocument('').documentElement;
        container.innerHTML = revisionInfo.responseText;

        let requests = [];
        const revisions = Array.from(container.querySelectorAll('#revisions tr'));
        revisions.forEach(element => {
          if (element.getAttribute('class') === 'revision') {
            const revisionDateInfo = element.querySelector('.revcell4 .relativetime');
            const date = revisionDateInfo.getAttribute('title');
            const userInfo = element.querySelector('.revcell4 .user-details > a');

            // Could be a deleted user
            if (!userInfo) {
              return;
            }

            const match = userInfo.getAttribute('href').match(/\/users\/(\-?\d+)/);
            const userId = parseInt(match[1], 10);

            const postTags = Array.from(element.nextElementSibling.querySelectorAll('.post-tag > span'));
            if (postTags.length <= 0) {
              return;
            }

            requests = requests.concat(postTags.map(e => {
              return {
                unknownDeletionId: postToVisit.unknownDeletionId,
                userId,
                tag: e.innerHTML,
                actionTypeId: e.getAttribute('class') === 'diff-delete' ? 1 : 2,
                dateTime: date
              };
            }));
          } else if (element.getAttribute('class') === 'vote-revision') {
            const revisionVoteInfo = element.querySelector('.revcell3');
            const xml = revisionVoteInfo.outerHTML;

            let actionTypeId = -1;
            if (xml.indexOf('<b>Post Closed</b>') > 0) {
              actionTypeId = 3;
            } else if (xml.indexOf('<b>Post Reopened</b>') > 0) {
              actionTypeId = 4;
            } else if (xml.indexOf('<b>Post Deleted</b>') > 0) {
              actionTypeId = 5;
            } else if (xml.indexOf('<b>Post Undeleted</b>') > 0) {
              actionTypeId = 6;
            }
            if (actionTypeId === -1) {
              return;
            }

            const revisionDateInfo = element.querySelector('.revcell4 .relativetime');
            const date = revisionDateInfo.getAttribute('title');

            let userIdMatch = this.userIdRegex.exec(xml);
            while (userIdMatch != null) {
              const userId = parseInt(userIdMatch[1], 10);
              requests.push({
                unknownDeletionId: postToVisit.unknownDeletionId,
                userId,
                actionTypeId,
                dateTime: date
              });
              userIdMatch = this.userIdRegex.exec(xml);
            }
          }
        });

        if (requests.length) {
          this.httpClient.post('/api/admin/ResolveUnresolvedDeletion', requests, {
            headers: { 'Authorization': 'Bearer ' + this.authDetails.RawToken }
          })
            .subscribe(_ => {
              postToVisit.status = 'done';
              setTimeout(processNext, 5000);
            });
        } else {
          console.error('Could not find any revisions for post ' + postToVisit.postId);
        }
      });
    };
    processNext();
  }
}
