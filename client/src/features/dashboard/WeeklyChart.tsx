import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  type ChartOptions,
  LinearScale,
  Tooltip,
} from 'chart.js'
import { useState } from 'react'
import { Bar } from 'react-chartjs-2'
import { todayKey } from '@/features/calendar/time'
import { formatPrice } from '@/shared/ui/format'
import { formatShortDay, formatWholePrice } from './format'
import type { DashboardDay } from './types'

// sadece kullanılan parçalar; registerables bütün grafik türlerini pakete katar
ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip)

// canvas CSS değişkenlerini okuyamaz; index.css'teki renklerle aynı
const CLAY = '#b8492f'
const CLAY_LIGHT = 'rgba(184, 73, 47, 0.3)'
const LINE = '#f0e8dd'
const MUTED = '#6b6259'
const FONT = "'Plus Jakarta Sans Variable', ui-sans-serif, sans-serif"

type Metric = 'count' | 'revenue'

interface WeeklyChartProps {
  days: DashboardDay[]
}

const tabButton = 'h-8 cursor-pointer rounded-full px-3 text-xs font-semibold'

export function WeeklyChart({ days }: WeeklyChartProps) {
  const [metric, setMetric] = useState<Metric>('count')
  const today = todayKey()

  const totalCount = days.reduce((sum, day) => sum + day.appointmentCount, 0)
  const totalRevenue = days.reduce((sum, day) => sum + day.revenue, 0)
  const values = days.map((day) => (metric === 'count' ? day.appointmentCount : day.revenue))

  const data = {
    labels: days.map((day) => formatShortDay(day.date)),
    datasets: [
      {
        data: values,
        // bugün koyu, geçmiş günler açık
        backgroundColor: days.map((day) => (day.date === today ? CLAY : CLAY_LIGHT)),
        borderRadius: 6,
        maxBarThickness: 36,
      },
    ],
  }

  const options: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        displayColors: false,
        titleFont: { family: FONT },
        bodyFont: { family: FONT },
        callbacks: {
          // iki bilgi birlikte; hangi sekme açık olursa olsun
          label: (context) => {
            const day = days[context.dataIndex]
            return [`${day.appointmentCount} randevu`, `${formatPrice(day.revenue)} ciro`]
          },
        },
      },
    },
    scales: {
      x: {
        grid: { display: false },
        ticks: { color: MUTED, font: { family: FONT } },
      },
      y: {
        beginAtZero: true,
        // hiç veri yokken eksen 0–1 olmasın; daha büyük değerde kendiliğinden genişler
        suggestedMax: metric === 'revenue' ? 1000 : 5,
        border: { display: false },
        grid: { color: LINE },
        ticks: {
          color: MUTED,
          font: { family: FONT },
          precision: 0,
          callback: (value) => (metric === 'revenue' ? formatWholePrice(Number(value)) : value),
        },
      },
    },
  }

  return (
    <section className="flex min-w-0 flex-col gap-4 rounded-card border border-line bg-surface p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-xl font-bold">Son 7 gün</h2>
          <span className="text-sm text-muted">
            {totalCount} randevu · {formatPrice(totalRevenue)} gerçekleşen ciro
          </span>
        </div>
        <div
          role="group"
          aria-label="Grafikte gösterilen"
          className="flex rounded-full bg-sand-soft p-1"
        >
          <button
            type="button"
            aria-pressed={metric === 'count'}
            onClick={() => setMetric('count')}
            className={`${tabButton} ${metric === 'count' ? 'bg-surface shadow-sm' : 'text-muted'}`}
          >
            Randevu
          </button>
          <button
            type="button"
            aria-pressed={metric === 'revenue'}
            onClick={() => setMetric('revenue')}
            className={`${tabButton} ${metric === 'revenue' ? 'bg-surface shadow-sm' : 'text-muted'}`}
          >
            Ciro
          </button>
        </div>
      </div>

      <div className="relative h-64">
        <Bar
          data={data}
          options={options}
          role="img"
          aria-label={`Son 7 günde ${totalCount} randevu ve ${formatPrice(totalRevenue)} ciro`}
        />
      </div>

      <p className="text-xs text-muted">İptaller sayılmaz; ciro sadece tamamlanan randevulardan.</p>
    </section>
  )
}
