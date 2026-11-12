import { DOCUMENT } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly document = inject(DOCUMENT);

  protected readonly title = signal('تهيئة نظام مراجعة المرتبات');
  protected readonly systemName = signal('PayrollReviewSystem');
  protected readonly stage = signal('BOOT-001 — Technical Scaffold');
  protected readonly targetDb = signal('SQL Server 2014 (12.x)');

  ngOnInit(): void {
    this.document.documentElement.lang = 'ar';
    this.document.documentElement.dir = 'rtl';
  }
}
