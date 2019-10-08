import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-intercepted-email',
  templateUrl: './intercepted-email.component.html'
})
export class InterceptedEmailComponent {
  emails: IEmail[];
  distinctFilters: IDistinctFilters;
  http: HttpClient;
  baseUrl: string;
  apiEmails: string;
  apiDistinctFilters: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.apiEmails = baseUrl + 'api/Emails/';
    this.apiDistinctFilters = baseUrl + 'api/DistinctFilters/';

    this.loadEmails(http, this.apiEmails);
    this.loadDistinctFilters(http, this.apiDistinctFilters);
  }

  loadEmails(http: HttpClient, baseUrl: string) {
    http
      .get<IEmail[]>(baseUrl + 'TopN/50')
      .subscribe(result => {
        this.emails = result;
      }, error => console.error(error));
  }

  loadDistinctFilters(http: HttpClient, baseUrl: string) {
    http
      .get<IDistinctFilters>(baseUrl)
      .subscribe(result => {
        this.distinctFilters = result;
      }, error => console.error(error));
  }

  onClickRefresh() {
    this.loadEmails(this.http, this.apiEmails);
  }

  onClickDeleteAll() {
    //I have to replace this with Angular Material later
    if (confirm("This will delete all email! Are you sure?")) {
      this.http
        .delete(this.apiEmails + "DeleteAll")
        .subscribe(() => console.log("Email table has been truncated"));

      this.emails = [];
    }
  }

  onClickFilter() {

  }
}

interface IEmail {
  emailId: number;
  from: string;
  to: string;
  subject: string;
  message: string;
  createdOnUtc: string;
}

interface IDistinctFilters {
  toAddresses: string[];
  fromAddresses: string[];
  subjects: string[];
}
