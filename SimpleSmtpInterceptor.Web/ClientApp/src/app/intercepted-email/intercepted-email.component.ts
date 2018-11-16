import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-intercepted-email',
  templateUrl: './intercepted-email.component.html'
})
export class InterceptedEmailComponent {
  emails: IEmail[];
  http: HttpClient;
  baseUrl: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl + 'api/Emails/';
    this.getEmails(http, this.baseUrl);
  }

  getEmails(http: HttpClient, baseUrl: string) {
    http
      .get<IEmail[]>(baseUrl + 'TopN/50')
      .subscribe(result => {
        this.emails = result;
      }, error => console.error(error));
  }

  onClickDeleteAll() {
    //I have to replace this with Angular Material later
    if (confirm("This will delete all email! Are you sure?")) {
      this.http
        .delete(this.baseUrl + "DeleteAll")
        .subscribe(() => console.log("Email table has been truncated"));

      this.emails = [];
    }
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
