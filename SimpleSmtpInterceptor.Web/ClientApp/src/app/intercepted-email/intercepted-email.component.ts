import { Component, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Component({
  selector: "app-intercepted-email",
  templateUrl: "./intercepted-email.component.html"
})
export class InterceptedEmailComponent {
  emails: IEmail[];
  distinctFilters: IDistinctFilters;
  http: HttpClient;
  baseUrl: string;
  apiEmails: string;
  apiDistinctFilters: string;
  emailFilter: EmailFilter; //Saved email filter that is being used
  colToggle: ColumnToggle;
  serverErrors: boolean;
  serverErrorMessage: string;

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.apiEmails = baseUrl + "api/Emails/";
    this.apiDistinctFilters = baseUrl + "api/DistinctFilters/";
    this.colToggle = new ColumnToggle();

    this.emailFilter = new EmailFilter();
    this.emailFilter.pageSize = 50;

    //This has to be loaded first
    this.loadDistinctFilters();
    this.loadEmails(this.emailFilter);

    console.dir(this.emailFilter);
  }

  loadEmails(emailFilter: EmailFilter) {
    const f = emailFilter;

    f.from = this.checkForBlank(f.from);
    f.to = this.checkForBlank(f.to);
    f.subject = this.checkForBlank(f.subject);

    this.http
      .get<IEmail[]>(this.apiEmails + "Filtered/" + f.pageSize + "/" + f.to + "/" + f.from + "/" + f.subject)
      .subscribe(result => {
        this.emails = result;
      }, error => this.handleError(error));
  }

  checkForBlank(target: string) {
    let str = target;

    if (target === "" || target == undefined) {
      str = "%20";
    }

    return str;
  }

  loadDistinctFilters() {
    this.http
      .get<IDistinctFilters>(this.apiDistinctFilters)
      .subscribe(result => {
        this.distinctFilters = result;
      }, error => this.handleError(error));
  }

  handleError(error: any) {
    this.serverErrors = true;

    let msg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';

    this.serverErrorMessage = msg;

    console.error(error);
  }

  onClickRefresh() {
    this.loadEmails(this.emailFilter);
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

  onClickFilter(emailFilter: EmailFilter) {
    this.emailFilter = emailFilter;

    this.onClickRefresh();
  }

  onClickShowAllColumns() {
    this.colToggle = new ColumnToggle();
  }
}

class ColumnToggle {
  constructor() {
    this.id = false;
    this.from = true;
    this.to = true;
    this.subject = true;
    this.messageText = false;
    this.messageHtml = true;
    this.attachmentCount = false;
    this.createdOnUtc = true;
  }

  id: boolean;
  from: boolean;
  to: boolean;
  subject: boolean;
  messageText: boolean;
  messageHtml: boolean;
  attachmentCount: boolean;
  createdOnUtc: boolean;
}

interface IEmail {
  emailId: number;
  from: string;
  to: string;
  subject: string;
  message: string;
  attachmentCount: number;
  createdOnUtc: string;
}

class EmailFilter {
  from: string;
  to: string;
  subject: string;
  pageSize: number;
}

interface IDistinctFilters {
  toAddresses: string[];
  fromAddresses: string[];
  subjects: string[];
}
